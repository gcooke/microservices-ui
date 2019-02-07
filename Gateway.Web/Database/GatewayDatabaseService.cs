﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using Bagl.Cib.MIT.IoC;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Models.Home;
using Gateway.Web.Models.Monitoring;
using Gateway.Web.Models.Request;
using Gateway.Web.Models.Security;
using Gateway.Web.Models.ServerResource;
using RestSharp.Extensions;
using WebGrease.Css.Ast.Selectors;
using QueueChartModel = Gateway.Web.Models.Controller.QueueChartModel;

namespace Gateway.Web.Database
{
    public class GatewayDatabaseService : IGatewayDatabaseService
    {
        public readonly string ConnectionString = String.Empty;

        public GatewayDatabaseService(ISystemInformation systemInformation)
        {
            ConnectionString = systemInformation.GetConnectionString("GatewayDatabase", "Database.PnRFO_Gateway");
        }

        public List<Models.Controllers.ControllerStats> GetControllerStatistics(DateTime start)
        {
            var result = new List<Models.Controllers.ControllerStats>();

            using (var database = new GatewayEntities(ConnectionString))
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
            using (var database = new GatewayEntities(ConnectionString))
            {
                return database.Controllers.OrderBy(c => c.Name).Select(x => x.Name).ToList();
            }
        }

        public List<HistoryItem> GetRecentRequests(DateTime start)
        {
            var result = new List<HistoryItem>();
            using (var database = new GatewayEntities(ConnectionString))
            {
                var items = database.spGetRecentRequestsAll(start, "");
                foreach (var item in items)
                {
                    result.Add(item.ToModel());
                }
            }
            return result;
        }

        public List<HistoryItem> GetRecentRequests(string controller, DateTime start, string search = null)
        {
            var result = new List<HistoryItem>();
            using (var database = new GatewayEntities(ConnectionString))
            {
                var items = database.spGetRecentRequests(start, controller).ToList();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    items = items
                        .Where(x => x.Resource.Contains(search))
                        .ToList();
                }

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
            using (var database = new GatewayEntities(ConnectionString))
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
            using (var database = new GatewayEntities(ConnectionString))
            {
                return new ResponseStats(database.spGetResponseStatsAll(start));
            }
        }

        public AliasesModel GetAliases()
        {
            var result = new AliasesModel();
            using (var database = new GatewayEntities(ConnectionString))
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

                var latestGroups = versions.Where(v => v.Status.Name.ToLower() == "active").GroupBy(v => v.ControllerId);
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
            using (var database = new GatewayEntities(ConnectionString))
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
            using (var database = new GatewayEntities(ConnectionString))
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
            using (var database = new GatewayEntities(ConnectionString))
            {
                var items = database.Links.ToArray().Select(l => l.ToModel());
                result.Items.AddRange(items);
            }
            return result;
        }

        public void DeleteLink(long id)
        {
            using (var database = new GatewayEntities(ConnectionString))
            {
                var item = database.Links.FirstOrDefault(l => l.Id == id);
                if (item != null)
                    database.Links.Remove(item);
                database.SaveChanges();
            }
        }

        public void AddLink(LinkModel link)
        {
            using (var database = new GatewayEntities(ConnectionString))
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
            using (var database = new GatewayEntities(ConnectionString))
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
            using (var database = new GatewayEntities(ConnectionString))
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
            using (var database = new GatewayEntities(ConnectionString))
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
            using (var database = new GatewayEntities(ConnectionString))
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
            using (var database = new GatewayEntities(ConnectionString))
            {
                var payload = database.Payloads.FirstOrDefault(p => p.Id == id);
                return new PayloadData(payload);
            }
        }

        public ReportsModel GetUsage()
        {
            var result = new ReportsModel("Usage Report");
            using (var database = new GatewayEntities(ConnectionString))
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

        public IEnumerable<ControllerState> GetControllerStates(IDictionary<string, ServerDiagnostics> serverDiagnostics)
        {
            var result = new List<ControllerState>();

            try
            {
                using (var database = new GatewayEntities(ConnectionString))
                {
                    var items = database.spGetControllerStates().ToList();
                    foreach (var item in items.GroupBy(i => i.Controller))
                    {
                        var controllerState = item.ToArray().ToModel();
                        SetControllerStatistics(controllerState, serverDiagnostics);
                        result.Add(controllerState);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }


            return result;
        }

        private void SetControllerStatistics(ControllerState controllerState, IDictionary<string, ServerDiagnostics> serverDiagnostics)
        {
            if (serverDiagnostics == null)
                return;

            foreach (var serverDiagnostic in serverDiagnostics.Values)
            {
                var controllerRequestsDiagnostic = serverDiagnostic.Requests.Where(x => x.Name.ToLower() == controllerState.Name.ToLower()).ToList();
                var controllerWorkerDiagnostic = serverDiagnostic.Workers.Where(x => x.Name.ToLower() == controllerState.Name.ToLower()).ToList();

                var requestCount = controllerRequestsDiagnostic.Sum(x => x.RequestCount);
                var inProgressRequestCount = controllerRequestsDiagnostic.Sum(x => x.RequestInProgressCount);
                var workerCount = controllerWorkerDiagnostic.Sum(x => x.WorkerCount);
                var workerInProgressCount = controllerWorkerDiagnostic.Sum(x => x.WorkerInProgressCount);

                controllerState.TotalRequestCount = requestCount;
                controllerState.ProcessingRequestCount = inProgressRequestCount;
                controllerState.BusyWorkerCount = workerInProgressCount;
                controllerState.TotalWorkerCount = workerCount;
            }
        }

        public IEnumerable<string> GetVersions(string controllerName)
        {
            using (var database = new GatewayEntities(ConnectionString))
            {
                var controller = database.Controllers.FirstOrDefault(x => x.Name.ToLower() == controllerName.ToLower());

                if (controller == null)
                    throw new ArgumentException($"Unable to locate controller named {controllerName}");

                return database.Versions.Where(x => x.ControllerId == controller.Id).Select(x => x.Version1).ToList();
            }
        }

        public IEnumerable<string> GetActiveVersions(string controllerName)
        {
            using (var database = new GatewayEntities(ConnectionString))
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
            using (var database = new GatewayEntities(ConnectionString))
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
            using (var database = new GatewayEntities(ConnectionString))
            {
                return database.Status.Where(s => s.Name != "Deleted").ToList();
            }
        }

        public bool HasStatusChanged(string controllerName, string versionName, string status, string alias)
        {
            using (var database = new GatewayEntities(ConnectionString))
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

        public QueueChartModel GetQueueChartModel(DateTime startDate)
        {
            var model = GetHistoricalCounts(startDate);
            return model.GetSummaryForAllControllers();
        }

        public QueueChartModel GetQueueChartModel(DateTime startDate, IList<string> controllers)
        {
            var model = GetHistoricalCounts(startDate);
            return model.GetSummaryForSelectedControllers(controllers);
        }

        public async Task<List<ExtendedBatchSummary>> GetBatchSummaryStatsAsync(DateTime @from, DateTime to)
        {
            var results = new List<ExtendedBatchSummary>();

            using (var database = new GatewayEntities(ConnectionString))
            {
                var batchStats = database.BatchStats;
                var requests = database.Requests;
                var responses = database.Responses;
                var dbSummary = await batchStats
                    .Where(x => DbFunctions.TruncateTime(x.ValuationDate) >= from &&
                                DbFunctions.TruncateTime(x.ValuationDate) < to
                                //&& x.Controller == "riskbatch"
                                )
                    .Join(requests, b => b.CorrelationId, r => r.CorrelationId, (b, r) => new { b, r })
                    .Join(responses, b => b.b.CorrelationId, r => r.CorrelationId,
                        (b, r) => new { batch = b.b, request = b.r, response = r })
                    .ToArrayAsync().ConfigureAwait(false);

                foreach (var item in dbSummary)
                {
                    var resource = item.request.Resource;

                    if (item.request.Controller.ToLower() != "riskbatch") // Remove
                        continue;

                    var scheduleId = long.Parse(resource.Split('/')[2]);
                    var schedule = database.Schedules.SingleOrDefault(x => x.ScheduleId == scheduleId);

                    if (schedule == null)
                        continue;

                    // Get all children
                    var children = GetChildRequests(database, item.request.CorrelationId);
                    ChildRequest[] pricingRequests, marketDataRequests, riskDataRequests, tradeStoreRequests;
                    if (!children.TryGetValue("pricing", out pricingRequests))
                        pricingRequests = new ChildRequest[0];


                    if (!children.TryGetValue("marketdata", out marketDataRequests)) //Count only 
                        marketDataRequests = new ChildRequest[0];
                    if (!children.TryGetValue("riskdata", out riskDataRequests)) //Count only 
                        riskDataRequests = new ChildRequest[0];
                    if (!children.TryGetValue("tradestore", out tradeStoreRequests)) // Sum Size 
                        tradeStoreRequests = new ChildRequest[0];

                    var pricingResults = new Dictionary<string, Tuple<int, int>>();
                    foreach (var pricingRequest in pricingRequests)
                    {

                        var calculationNameUnmapped = GetCalculationName(pricingRequest.Resource);
                        var calculationName = TransformCalculationName(calculationNameUnmapped);

                        if (pricingResults.ContainsKey(calculationName))
                            continue;

                        var totalRequests = pricingRequests.Count(x => x.Resource.EndsWith(calculationNameUnmapped));
                        var successfulRequests = pricingRequests.Count(x => x.ResultCode == 1 && x.Resource.EndsWith(calculationNameUnmapped));

                        pricingResults.Add(calculationName, new Tuple<int, int>(successfulRequests, totalRequests));
                    }

                    var summary = item.batch.ToModel(item.request, item.response);
                    summary.CalculationPricingRequestResults = pricingResults;
                    if (schedule.RiskBatchSchedule?.TradeSourceType == "Portfolio")
                        summary.Name = schedule.Name;
                    else
                        summary.Name = schedule.RiskBatchSchedule?.RiskBatchConfiguration.Type;

                    summary.Trades = tradeStoreRequests.Sum(r => r.Size);
                    summary.PricingRequests = pricingRequests.Length;
                    summary.MarketDataRequests = marketDataRequests.Length;
                    summary.RiskDataRequests = riskDataRequests.Length;

                    results.Add(summary);
                }
                return results;
            }
        }

        private Dictionary<string, ChildRequest[]> GetChildRequests(GatewayEntities database, Guid correlationId)
        {
            return database.Requests
                .Join(database.Responses, r => r.CorrelationId, rs => rs.CorrelationId,
                    (rq, rs) => new { request = rq, response = rs })
                .Where(x => x.request.ParentCorrelationId == correlationId)
                .ToArray()
                .Select(x => new ChildRequest(x.request.CorrelationId, x.request.Controller, x.request.Resource, x.response.ResultCode, x.response.Size))
                .GroupBy(c => c.Controller)
                .ToDictionary(g => g.Key, g => g.ToArray());
        }

        public ResourceConfigModel GetConfiguredServers()
        {
            using (var database = new GatewayEntities(ConnectionString))
            {
                ResourceConfigModel model = new ResourceConfigModel();


                model.Configs = database.Servers.Select(s => new ItemResourceConfig
                {
                    Name = s.Name,
                    DisplayName = s.Name + "/" + s.Domain,
                    AllowableResources = s.ServerExternalResources.Select(ser => new ExternalResourceConfig
                    {
                        Name = ser.ExternalResource.Name,
                        Type = ser.ExternalResource.Type
                    }).ToList()
                }).ToList();


                model.AllResources = database.ExternalResources.Select(er => new ExternalResourceConfig
                {
                    Name = er.Name,
                    Type = er.Type
                }).ToList();

                model.ConfigType = ResourceConfigType.Server;

                return model;
            }
        }

        public ResourceConfigModel GetControllerResources()
        {
            using (var database = new GatewayEntities(ConnectionString))
            {
                ResourceConfigModel model = new ResourceConfigModel();

                model.Configs = database.Controllers.Select(s => new ItemResourceConfig
                {
                    Name = s.Name,
                    DisplayName = s.Name,
                    AllowableResources = s.ControllerExternalResources.Select(ser => new ExternalResourceConfig
                    {
                        Name = ser.ExternalResource.Name,
                        Type = ser.ExternalResource.Type
                    }).ToList()
                }).ToList();


                model.AllResources = database.ExternalResources.Select(er => new ExternalResourceConfig
                {
                    Name = er.Name,
                    Type = er.Type
                }).ToList();

                model.ConfigType = ResourceConfigType.Controller;

                return model;
            }
        }


        public void DeleteServerResourceLink(string serverName, string resourceName)
        {
            using (var database = new GatewayEntities(ConnectionString))
            {
                var toDelete = database.ServerExternalResources.Where(sr =>
                    sr.ExternalResource.Name == resourceName && sr.Server.Name == serverName);

                database.ServerExternalResources.RemoveRange(toDelete);

                database.SaveChanges();
            }
        }

        public void AddServerResourceLink(string serverName, string resourceName)
        {
            using (var database = new GatewayEntities(ConnectionString))
            {
                var existing = database.ServerExternalResources.Where(sr =>
                    sr.ExternalResource.Name == resourceName && sr.Server.Name == serverName);

                if (!existing.Any())
                {
                    var server = database.Servers.SingleOrDefault(s => s.Name == serverName);
                    var resource = database.ExternalResources.SingleOrDefault(r => r.Name == resourceName);

                    if (server != null && resource != null)
                    {
                        database.ServerExternalResources.Add(new ServerExternalResource
                        {
                            ExternalResource = resource,
                            Server = server
                        });
                        database.SaveChanges();
                    }
                }

            }

        }

        public void DeleteControllerResourceLink(string controllerName, string resourceName)
        {
            using (var database = new GatewayEntities(ConnectionString))
            {
                var toDelete = database.ControllerExternalResources.Where(sr =>
                    sr.ExternalResource.Name == resourceName && sr.Controller.Name == controllerName);

                database.ControllerExternalResources.RemoveRange(toDelete);

                database.SaveChanges();
            }
        }

        public void AddControllerResourceLink(string controllerName, string resourceName)
        {
            using (var database = new GatewayEntities(ConnectionString))
            {
                var existing = database.ControllerExternalResources.Where(sr =>
                    sr.ExternalResource.Name == resourceName && sr.Controller.Name == controllerName);

                if (!existing.Any())
                {
                    var controller = database.Controllers.SingleOrDefault(s => s.Name == controllerName);
                    var resource = database.ExternalResources.SingleOrDefault(r => r.Name == resourceName);

                    if (controller != null && resource != null)
                    {
                        database.ControllerExternalResources.Add(new ControllerExternalResource
                        {
                            ExternalResource = resource,
                            Controller = controller
                        });
                        database.SaveChanges();
                    }
                }

            }
        }

        private HistoricalSummary GetHistoricalCounts(DateTime start)
        {
            using (var database = new GatewayEntities(ConnectionString))
            {
                var results = database.spGetRequestCounts(start)
                                      .Select(r => r.ToModel())
                                      .ToArray();
                return new HistoricalSummary(results);
            }
        }

        private string TransformCalculationName(string name)
        {
            if (name.ToLower() == "pv")
                return "PV";

            if (name.ToLower().StartsWith("xvacalculationwith"))
                return "XVA";

            if (name.ToLower() == "generalriskreport")
                return "Risk";

            return name;
        }

        private string GetCalculationName(string resource)
        {
            if (resource == null)
                return null;

            var lastIndex = resource.LastIndexOf('-');
            return resource.Substring(lastIndex + 1).Trim();
        }
    }

    internal class ChildRequest
    {
        public Guid CorrelationId { get; }
        public string Resource { get; }
        public string Controller { get; }
        public int ResultCode { get; }
        public int? Size { get; }

        public ChildRequest(Guid correlationId, string controller, string resource, int resultCode, int? size)
        {
            CorrelationId = correlationId;
            Controller = controller;
            Resource = resource;
            ResultCode = resultCode;
            Size = size;
        }
    }

    internal class ControllerVersionSummaryQueueSize
    {
        public string Controller { get; set; }
        //public string Version { get; set; }
        public int Count { get; set; }
    }

    public class QueueSizeModel
    {
        public DateTime Time { get; set; }
        public string Label { get; set; }
        public int Count { get; set; }
    }

}