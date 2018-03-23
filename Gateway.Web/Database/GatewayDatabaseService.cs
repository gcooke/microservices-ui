using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Models.Request;
using Gateway.Web.Models.Security;
using RestSharp.Extensions;
using WebGrease.Css.Ast.Selectors;
using QueueChartModel = Gateway.Web.Models.Controller.QueueChartModel;

namespace Gateway.Web.Database
{
    public class GatewayDatabaseService : IGatewayDatabaseService
    {
        public GatewayDatabaseService()
        {
            //PnRFO_GatewayEntities
        }

        public List<Models.Controllers.ControllerStats> GetControllerStatistics(DateTime start)
        {
            var result = new List<Models.Controllers.ControllerStats>();

            using (var database = new GatewayEntities())
            {
                // Get stats
                var stats = new ResponseStats(database.spGetResponseStatsAll(start));

                foreach (var controller in database.Controllers.OrderBy(c => c.Name))
                {
                    var model = controller.ToModel(stats);
                    result.Add(model);
                }
            }

            // Insert alphabet
            var previous = ' ';
            for (int index = 0; index < result.Count; index++)
            {
                var current = result[index].Name[0];
                if (current != previous)
                {
                    result.Insert(index, new ControllerStats() { Name = current.ToString(), IsSeperator = true });
                    index++;
                }
                previous = current;
            }

            return result;
        }

        public List<string> GetControllerNames()
        {
            using (var database = new GatewayEntities())
            {
                return database.Controllers.OrderBy(c => c.Name).Select(x => x.Name).ToList();
            }
        }

        public List<HistoryItem> GetRecentRequests(DateTime start)
        {
            var result = new List<HistoryItem>();
            using (var database = new GatewayEntities())
            {
                var items = database.spGetRecentRequestsAll(start, "");
                foreach (var item in items)
                {
                    result.Add(item.ToModel());
                }
            }
            return result;
        }

        public List<HistoryItem> GetRecentRequests(string controller, DateTime start)
        {
            var result = new List<HistoryItem>();
            using (var database = new GatewayEntities())
            {
                var items = database.spGetRecentRequests(start, controller);
                foreach (var item in items)
                {
                    result.Add(item.ToModel());
                }
            }
            return result;
        }

        public List<HistoryItem> GetRecentUserRequests(string user, DateTime start)
        {
            var result = new List<HistoryItem>();
            using (var database = new GatewayEntities())
            {
                var items = database.spGetRecentUserRequests(start, user);
                foreach (var item in items)
                {
                    result.Add(item.ToModel());
                }
            }
            return result;
        }

        public ResponseStats GetResponseStats(DateTime start)
        {
            using (var database = new GatewayEntities())
            {
                return new ResponseStats(database.spGetResponseStatsAll(start));
            }
        }

        public AliasesModel GetAliases()
        {
            var result = new AliasesModel();
            using (var database = new GatewayEntities())
            {
                var versions = database.Versions.Where(v => v.Status.Name != "Deleted").ToArray();
                foreach (var version in versions)
                {
                    if (string.IsNullOrEmpty(version.Alias)) continue;

                    foreach (var aliasName in version.Alias.Split(','))
                    {
                        var alias = result.GetOrAdd(aliasName);
                        alias.Controllers.Add(new ControllerVersion(version.Controller.Name, version.Version1,
                            version.Status.Name, version.Status.IsActive));
                    }
                }

                var latestGroups = versions.Where(v => v.StatusId == 2).GroupBy(v => v.ControllerId);
                var latest = result.GetOrAdd(" Latest");
                foreach (var group in latestGroups)
                {
                    var version = group.OrderBy(v => System.Version.Parse(v.Version1)).Last();
                    latest.Controllers.Add(new ControllerVersion(version.Controller.Name, version.Version1,
                        version.Status.Name, version.Status.IsActive));
                }
            }
            return result;
        }

        public RequestsChartModel GetControllerRequestSummary(string name, DateTime start)
        {
            using (var database = new GatewayEntities())
            {
                var controller = database.Controllers.FirstOrDefault(c => c.Name == name);
                if (controller == null)
                    return new RequestsChartModel("Unknown controller");

                // Get recent requests
                var results = database.spGetRequestStats(start, name);
                var result = new RequestsChartModel(name);
                result.Name = name;
                foreach (var item in results.OrderBy(r => r.Date))
                {
                    if (item.Count != null && item.Date != null)
                    {
                        var label = item.Date.Value.ToString("dd MMM");
                        result.RequestSummary.Add(new ChartItem(label, item.Count.Value));
                    }
                }

                return result;
            }
        }

        public TimeChartModel GetControllerTimeSummary(string name, DateTime start)
        {
            using (var database = new GatewayEntities())
            {
                var controller = database.Controllers.FirstOrDefault(c => c.Name == name);
                if (controller == null)
                    return new TimeChartModel("Unknown controller");

                // Get recent requests
                var results = database.spGetTimeStats(start, name);
                var result = new TimeChartModel(name);
                result.Name = name;
                foreach (var item in results.OrderBy(r => r.Date).ThenBy(r => TimeSpan.Parse(r.Hour)))
                {
                    var label = item.Date.Value.ToString("dd MMM") + " " + item.Hour;
                    var value = item.AvgTime.Value;
                    var value2 = item.MaxTime.Value;
                    result.TimeSummary.Add(new ChartItem(label, value, value2));
                }

                return result;
            }
        }

        public LinksModel GetLinks()
        {
            var result = new LinksModel();
            using (var database = new GatewayEntities())
            {
                var items = database.Links.ToArray().Select(l => l.ToModel());
                result.Items.AddRange(items);
            }
            return result;
        }

        public void DeleteLink(long id)
        {
            using (var database = new GatewayEntities())
            {
                var item = database.Links.FirstOrDefault(l => l.Id == id);
                if (item != null)
                    database.Links.Remove(item);
                database.SaveChanges();
            }
        }

        public void AddLink(LinkModel link)
        {
            using (var database = new GatewayEntities())
            {
                var target = database.Links.Create();
                target.Name = link.Name;
                target.Type = link.Type;
                target.Glyph = link.Glyph;
                target.AdditionalData = link.AdditionalData;
                target.Permission = link.Permission;
                database.Links.Add(target);
                database.SaveChanges();
            }
        }

        public Summary GetRequestSummary(string correlationId)
        {
            var result = new Summary();
            var id = Guid.Parse(correlationId);
            using (var database = new GatewayEntities())
            {
                var request = database.Requests.FirstOrDefault(r => r.CorrelationId == id);
                var response = database.Responses.FirstOrDefault(r => r.CorrelationId == id);
                PopulateFields(result, request);
                PopulateFields(result, response);
                result.CorrelationId = id;

                if (result.EndUtc > DateTime.MinValue)
                    result.WallClockTime = (result.EndUtc - result.StartUtc).ToString("h'h 'm'm 's's'");
                foreach (var item in database.spGetRequestChildSummary(id))
                    result.Items.Add(item.ToModel());
            }
            return result;
        }

        public List<HistoryItem> GetRequestChildren(Guid correlationId)
        {
            var result = new List<HistoryItem>();
            using (var database = new GatewayEntities())
            {
                var items = database.spGetChildRequests(correlationId);
                foreach (var item in items)
                {
                    result.Add(item.ToModel());
                }
            }
            return result;
        }

        public Payloads GetRequestPayloads(string correlationId)
        {
            var result = new Payloads();
            var id = Guid.Parse(correlationId);
            using (var database = new GatewayEntities())
            {
                var request = database.Requests.FirstOrDefault(r => r.CorrelationId == id);
                result.CorrelationId = id;
                if (request != null)
                {
                    result.Controller = request.Controller;
                    result.Version = request.Version;
                }
                foreach (var item in database.spGetPayloads(id))
                {
                    result.Items.Add(new PayloadModel(item));
                }
            }

            if (result.Items.Count < 2)
            {
                if (result.Items.All(i => i.Direction != "Response"))
                {
                    result.Items.Add(new PayloadModel("Response"));
                }
                if (result.Items.All(i => i.Direction != "Request"))
                {
                    result.Items.Insert(0, new PayloadModel("Request"));
                }
            }
            return result;
        }

        public Transitions GetRequestTransitions(string correlationId)
        {
            var result = new Transitions();
            var id = Guid.Parse(correlationId);
            result.CorrelationId = id;
            using (var database = new GatewayEntities())
            {
                var items = database.RequestChanges.Where(r => r.CorrelationId == id).ToArray();
                foreach (var item in items)
                {
                    result.Items.Add(item.ToModel());
                }
            }
            return result;
        }

        public PayloadData GetPayload(long id)
        {
            using (var database = new GatewayEntities())
            {
                var payload = database.Payloads.FirstOrDefault(p => p.Id == id);
                return new PayloadData(payload);
            }
        }

        public ReportsModel GetUsage()
        {
            var result = new ReportsModel("Usage Report");
            using (var database = new GatewayEntities())
            {
                var rows = database.spGetUserReport(DateTime.Today.AddDays(-7));
                var target = new UserRecentRequests();
                var table = new ReportTable();
                table.Title = "Recent Requests";
                table.Columns.Add("User");
                table.Columns.Add("Last Request");
                table.Columns.Add("Last 60 Minutes");
                table.Columns.Add("Last 24 Hours");
                table.Columns.Add("Last 7 Days");
                table.Columns.Add("Groups");

                foreach (var row in rows)
                {
                    target.Add(row);
                }

                foreach (var line in target.GetAll())
                {
                    var reportRow = new ReportRows();
                    var fulluser = line.User;
                    var user = fulluser;
                    if (fulluser.Contains("\\"))
                        user = user.Substring(fulluser.IndexOf("\\") + 1);
                    var link = string.Format("<a href='../../User/History?id=0&login={0}'>{1}</a>", fulluser, user);

                    reportRow.Values.Add(link);
                    reportRow.Values.Add(line.Latest.ToString("dd MMM HH:mm:ss"));
                    reportRow.Values.Add(line.Total60Minutes.ToString());
                    reportRow.Values.Add(line.Total24Hours.ToString());
                    reportRow.Values.Add(line.Total7Days.ToString());
                    reportRow.Values.Add(line.Groups);
                    table.Rows.Add(reportRow);

                }
                result.Tables.Add(table);
            }
            return result;
        }

        private void PopulateFields(Summary result, Response response)
        {
            if (response == null) return;

            result.EndUtc = response.EndUtc;
            result.QueueTimeMs = response.QueueTimeMs;
            result.TimeTakenMs = response.TimeTakeMs;
            result.ResultCode = response.ResultCode;
            result.ResultMessage = response.ResultMessage;
            result.UpdateTime = response.UpdateTime;
        }

        private void PopulateFields(Summary result, Request request)
        {
            if (request == null) return;
            result.ParentCorrelationId = request.ParentCorrelationId;
            result.User = request.User;
            result.IpAddress = request.IpAddress;
            result.Controller = request.Controller;
            result.Version = request.Version;
            result.Resource = request.Resource;
            result.RequestType = request.RequestType;
            result.Priority = request.Priority;
            result.IsAsync = request.IsAsync;
            result.StartUtc = request.StartUtc;
            result.Client = string.IsNullOrEmpty(request.ClientID) ? "Unknown" : request.ClientID;
        }

        public IEnumerable<string> GetVersions(string controllerName)
        {
            using (var database = new GatewayEntities())
            {
                var controller = database.Controllers.FirstOrDefault(x => x.Name.ToLower() == controllerName.ToLower());

                if (controller == null)
                    throw new ArgumentException($"Unable to locate controller named {controllerName}");

                return database.Versions.Where(x => x.ControllerId == controller.Id).Select(x => x.Version1).ToList();
            }
        }

        public IEnumerable<string> GetActiveVersions(string controllerName)
        {
            using (var database = new GatewayEntities())
            {
                var controller = database.Controllers.FirstOrDefault(x => x.Name.ToLower() == controllerName.ToLower());

                if (controller == null)
                    throw new ArgumentException($"Unable to locate controller named {controllerName}");

                var activeStatus = database.Status.SingleOrDefault(x => x.Name.ToLower() == "Active");

                if (activeStatus == null)
                {
                    throw new ArgumentException($"Unable to find status with name Active");
                }

                return database.Versions.Where(x => x.ControllerId == controller.Id && x.StatusId == activeStatus.Id).Select(x => x.Version1).ToList();
            }
        }

        public IEnumerable<string> GetActiveVersions()
        {
            using (var database = new GatewayEntities())
            {
                var activeStatus = database.Status.SingleOrDefault(x => x.Name.ToLower() == "Active");

                if (activeStatus == null)
                {
                    throw new ArgumentException($"Unable to find status with name Active");
                }

                return database.Versions.Where(x => x.StatusId == activeStatus.Id).Select(x => x.Version1).ToList();
            }
        }


        public IEnumerable<Status> GetVersionStatuses()
        {
            using (var database = new GatewayEntities())
            {
                return database.Status.Where(s => s.Name != "Deleted").ToList();
            }
        }

        public bool HasStatusChanged(string controllerName, string versionName, string status, string alias)
        {
            using (var database = new GatewayEntities())
            {
                var controller = database.Controllers
                    .Where(c => c.Name == controllerName).Select(c => c).First();

                if (controller != null)
                {
                    var version = controller.Versions
                        .Where(v => v.Version1 == versionName).Select(v => v).FirstOrDefault();

                    if (version != null)
                    {
                        return status != version.Status.Name || alias != version.Alias;
                    }
                    return status != "Inactive";
                }
                return false;
            }
        }

        public IDictionary<string, int> GetCurrentControllerQueueSize(DateTime endDateTime, IList<string> controllers)
        {
            using (var database = new GatewayEntities())
            {
                var data = GetCurrentControllerQueueSize(database, endDateTime);

                var selectedControllersSizes = from d in data
                                               where controllers.Contains(d.Controller)
                                               group d by d.Controller into g
                                               select new
                                               {
                                                   Controller = g.Key,
                                                   CurrentQueueCount = g.Sum(x => x.Count)
                                               };

                var otherControllersSizes = from d in data
                                            where !controllers.Contains(d.Controller)
                                            group d by 1 into g
                                            select new
                                            {
                                                Controller = "other",
                                                CurrentQueueCount = g.Sum(x => x.Count)
                                            };

                return selectedControllersSizes.Union(otherControllersSizes)
                    .ToDictionary(p => p.Controller, p => p.CurrentQueueCount);
            }
        }

        public IDictionary<string, int> GetCurrentControllerQueueSize(DateTime endDateTime)
        {
            using (var database = new GatewayEntities())
            {
                var data = GetCurrentControllerQueueSize(database, endDateTime).OrderByDescending(x => x.Count);

                var selectedControllersSizes = from d in data.Take(5)
                                               group d by d.Controller into g
                                               select new
                                               {
                                                   Controller = g.Key,
                                                   CurrentQueueCount = g.Sum(x => x.Count)
                                               };

                var otherControllersSizes = from d in data.Skip(5)
                                            group d by 1 into g
                                            select new
                                            {
                                                Controller = "other",
                                                CurrentQueueCount = g.Sum(x => x.Count)
                                            };

                return selectedControllersSizes.Union(otherControllersSizes)
                    .ToDictionary(p => p.Controller, p => p.CurrentQueueCount);
            }
        }

        private static IQueryable<ControllerVersionSummaryQueueSize> GetCurrentControllerQueueSize(GatewayEntities database, DateTime endDateTime)
        {
            var startTime = endDateTime.AddMinutes(-1);
            return from qs in database.QueueSizes
                   where qs.UpdateTime >= startTime && qs.UpdateTime < endDateTime
                   group qs by new { qs.Controller, qs.Version } into g
                   select new ControllerVersionSummaryQueueSize
                   {
                       Controller = g.Key.Controller,
                       Version = g.Key.Version,
                       Count = g.Max(x => x.ItemCount)
                   };
        }

        public QueueChartModel GetHistoricalControllerQueueSizes(DateTime endDateTime)
        {
            using (var database = new GatewayEntities())
            {
                var startTime = endDateTime.AddHours(-24);
                startTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, 0, 0);

                var query = from qs in database.QueueSizes
                            where qs.UpdateTime >= startTime
                            group qs by qs.Controller into g
                            select new
                            {
                                Controller = g.Key,
                                MaxItemCount = g.Max(x => x.ItemCount)
                            };
                var controllers = query.OrderByDescending(x => x.MaxItemCount).Take(5).Select(x => x.Controller).ToArray();
                return GetHistoricalControllerQueueSizes(endDateTime, controllers);
            }
        }

        public QueueChartModel GetHistoricalControllerQueueSizes(DateTime endDateTime, IList<string> controllers)
        {
            using (var database = new GatewayEntities())
            {
                var selectedControllers = GetHistoricalControllerQueueSizes(database, endDateTime, controllers.ToArray());
                var otherControllers = GetHistoricalControllerQueueSizes(database, endDateTime, controllers.ToArray(), "other");

                return new QueueChartModel(selectedControllers.Union(otherControllers).OrderBy(x => x.Time).ToList());
            }
        }

        private static IQueryable<QueueSizeModel> GetHistoricalControllerQueueSizes(GatewayEntities database, DateTime endDateTime, string[] controllers, string versionLabel)
        {
            var startDateTime = endDateTime.AddHours(-24);
            startDateTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, 0, 0);
            endDateTime = new DateTime(endDateTime.Year, endDateTime.Month, endDateTime.Day, endDateTime.Hour, 0, 0);

            return from qs in database.QueueSizes
                   where qs.UpdateTime >= startDateTime && qs.UpdateTime < endDateTime && controllers.Contains(qs.Controller)
                   group qs by new
                   {
                       Time = DbFunctions.CreateDateTime(qs.UpdateTime.Year, qs.UpdateTime.Month, qs.UpdateTime.Day, qs.UpdateTime.Hour, 0, 0).Value
                   } into g
                   select new QueueSizeModel
                   {
                       Label = versionLabel,
                       Time = g.Key.Time,
                       Count = g.Max(x => x.ItemCount)
                   };
        }

        private static IQueryable<QueueSizeModel> GetHistoricalControllerQueueSizes(GatewayEntities database, DateTime endDateTime, string[] controllers)
        {
            var startDateTime = endDateTime.AddHours(-24);
            startDateTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, 0, 0);
            endDateTime = new DateTime(endDateTime.Year, endDateTime.Month, endDateTime.Day, endDateTime.Hour, 0, 0);

            return from qs in database.QueueSizes
                   where qs.UpdateTime >= startDateTime && qs.UpdateTime < endDateTime && controllers.Contains(qs.Controller)
                   group qs by new
                   {
                       qs.Controller,
                       Time = DbFunctions.CreateDateTime(qs.UpdateTime.Year, qs.UpdateTime.Month, qs.UpdateTime.Day, qs.UpdateTime.Hour, 0, 0).Value
                   } into g
                   select new QueueSizeModel
                   {
                       Label = g.Key.Controller,
                       Time = g.Key.Time,
                       Count = g.Max(x => x.ItemCount)
                   };
        }

        public QueueChartModel GetHistoricalControllerVersionQueueSizes(DateTime endDateTime, string controller)
        {
            using (var database = new GatewayEntities())
            {
                var startTime = endDateTime.AddHours(-24);
                startTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, 0, 0);
                var activeVersions = GetActiveVersions(controller);

                var query = from qs in database.QueueSizes.Where(x => activeVersions.Contains(x.Version))
                            where qs.Controller.ToLower() == controller.ToLower() && qs.UpdateTime >= startTime
                            group qs by qs.Version into g
                            select new
                            {
                                Version = g.Key,
                                MaxItemCount = g.Max(x => x.ItemCount)
                            };
                var versions = query.OrderByDescending(x => x.MaxItemCount).Take(5).Select(x => x.Version).ToArray();
                return GetHistoricalControllerVersionQueueSizes(endDateTime, controller, versions);
            }
        }

        public QueueChartModel GetHistoricalControllerVersionQueueSizes(DateTime endDateTime, string controller, string[] versions)
        {
            using (var database = new GatewayEntities())
            {
                var startTime = endDateTime.AddHours(-24);
                startTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, 0, 0);

                var selectedVersions = GetHistoricalControllerVersionQueueSizes(database, startTime, controller, versions);
                var otherVersions = GetHistoricalControllerVersionQueueSizes(database, startTime, controller, versions, "other");

                return new QueueChartModel(selectedVersions.Union(otherVersions).OrderBy(x => x.Time).ToList());
            }
        }

        private static IQueryable<QueueSizeModel> GetHistoricalControllerVersionQueueSizes(GatewayEntities database, DateTime startTime, string controller, string[] versions)
        {
            return database.QueueSizes
                .Where(qs => qs.Controller.ToLower() == controller.ToLower() && versions.Contains(qs.Version))
                .GroupBy(qs => new
                {
                    qs.Version,
                    Time = DbFunctions.CreateDateTime(qs.UpdateTime.Year, qs.UpdateTime.Month, qs.UpdateTime.Day, qs.UpdateTime.Hour, 0, 0).Value
                })
                .Where(qs => qs.Key.Time >= startTime)
                .OrderByDescending(qs => qs.Key.Time)
                .Select(qs => new QueueSizeModel
                {
                    Label = qs.Key.Version,
                    Time = qs.Key.Time,
                    Count = qs.Max(x => x.ItemCount)
                });
        }

        private static IQueryable<QueueSizeModel> GetHistoricalControllerVersionQueueSizes(GatewayEntities database, DateTime startTime, string controller, string[] versions, string versionLabel)
        {
            return database.QueueSizes
                .Where(qs => qs.Controller.ToLower() == controller.ToLower() && !versions.Contains(qs.Version))
                .GroupBy(qs => new
                {
                    Time = DbFunctions.CreateDateTime(qs.UpdateTime.Year, qs.UpdateTime.Month, qs.UpdateTime.Day, qs.UpdateTime.Hour, 0, 0).Value
                })
                .Where(qs => qs.Key.Time >= startTime)
                .OrderByDescending(qs => qs.Key.Time)
                .Select(qs => new QueueSizeModel
                {
                    Label = versionLabel,
                    Time = qs.Key.Time,
                    Count = qs.Max(x => x.ItemCount)
                });
        }

        public LiveQueueChartModel GetLiveControllerVersionQueueSizes(DateTime startDateTime, DateTime? endDateTime, string controllerName)
        {
            var activeVersions = GetActiveVersions(controllerName);
            return GetLiveControllerVersionQueueSizes(startDateTime, endDateTime, controllerName, activeVersions.ToArray());
        }

        public LiveQueueChartModel GetLiveControllerVersionQueueSizes(DateTime startDateTime, DateTime? endDateTime, string controller, string[] versions)
        {
            using (var database = new GatewayEntities())
            {
                var selectedVersionsQuery = database.QueueSizes.Where(qs => qs.Controller.ToLower() == controller.ToLower() &&
                                                            versions.Contains(qs.Version) &&
                                                            qs.UpdateTime >= startDateTime &&
                                                            qs.UpdateTime < endDateTime);

                var otherVersionsQuery = database.QueueSizes.Where(qs => qs.Controller.ToLower() == controller.ToLower() &&
                                                 !versions.Contains(qs.Version) &&
                                                 qs.UpdateTime >= startDateTime &&
                                                 qs.UpdateTime < endDateTime)
                                             .GroupBy(x => x.UpdateTime);

                var data = selectedVersionsQuery.Select(qs => new QueueSizeModel
                {
                    Time = qs.UpdateTime,
                    Count = qs.ItemCount,
                    Label = qs.Version
                });

                data = data.Union(otherVersionsQuery.Select(qs => new QueueSizeModel
                {
                    Time = qs.Key,
                    Count = qs.Sum(x => x.ItemCount),
                    Label = "other"
                }));

                return new LiveQueueChartModel(data.ToList());
            }
        }

    }

    internal class ControllerVersionSummaryQueueSize
    {
        public string Controller { get; set; }
        public string Version { get; set; }
        public int Count { get; set; }
    }

    public class QueueSizeModel
    {
        public DateTime Time { get; set; }
        public string Label { get; set; }
        public int Count { get; set; }
    }

}