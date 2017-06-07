using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Request;

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

                foreach (var controller in database.Controllers)
                {
                    var model = controller.ToModel(stats);
                    result.Add(model);
                }
            }

            return result;
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

        public RequestModel GetRequestDetails(string correlationId)
        {
            var result = new RequestModel();
            var id = Guid.Parse(correlationId);
            using (var database = new GatewayEntities())
            {
                var request = database.Requests.FirstOrDefault(r => r.CorrelationId == id);
                var response = database.Responses.FirstOrDefault(r => r.CorrelationId == id);
                PopulateFields(result, request);
                PopulateFields(result, response);
                result.CorrelationId = id;                
                foreach (var item in database.spGetPayloads(id))
                {
                    result.Items.Add(new PayloadModel(item));
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

        private void PopulateFields(RequestModel result, Response response)
        {
            if (response == null) return;

            result.EndUtc = response.EndUtc;
            result.QueueTimeMs = response.QueueTimeMs;
            result.TimeTakenMs = response.TimeTakeMs;
            result.ResultCode = response.ResultCode;
            result.ResultMessage = response.ResultMessage;
            result.UpdateTime = response.UpdateTime;
        }

        private void PopulateFields(RequestModel result, Request request)
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
        }
        
        public Models.Controller.QueueChartModel GetControllerQueueSummary(string name, DateTime start)
        {
            using (var database = new GatewayEntities())
            {
                var results = database.spGetQueueCounts(start, name);
                var result = new Models.Controller.QueueChartModel(name);

                foreach (var group in results.GroupBy(r => r.Version))
                {
                    var series = new Models.Controller.VersionQueueChartModel(group.Key);
                    foreach (var value in group)
                    {
                        var updateTime = value.Date.Value.Add(TimeSpan.Parse(value.Hour));
                        var item = new Models.Controller.QueueCount(group.Key, updateTime, value.LastEnqueue, value.LastDequeue, value.ItemCount.Value);
                        series.Items.Add(item);
                    }

                    result.Versions.Add(series);
                }

                return result;
            }
        }

        public Models.Controllers.QueueChartModel GetControllerQueueSummary(DateTime start)
        {
            using (var database = new GatewayEntities())
            {
                var results = database.spGetQueueCountsAll(start);
                var result = new Models.Controllers.QueueChartModel();

                foreach (var group in results.GroupBy(r => string.Format("{0} ({1})", r.Controller, r.Version)))
                {
                    var last = group.Last();
                    var series = new Models.Controllers.VersionQueueChartModel(last.Controller, last.Version);
                    foreach (var value in group)
                    {
                        var updateTime = value.Date.Value.Add(TimeSpan.Parse(value.Hour));
                        var item = new Models.Controllers.QueueCount(group.Key, updateTime, value.LastEnqueue, value.LastDequeue, value.ItemCount.Value);
                        series.Items.Add(item);
                    }
                    result.Versions.Add(series);
                }

                return result;
            }
        }
        
        public IEnumerable<Status> GetVersionStatuses()
        {
            using (var database = new GatewayEntities())
            {
                return database.Status.Where(s => s.Name != "Deleted").ToList();
            }
        }

        public bool HasStatusChanged(string controllerName, string versionName, string status)
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
                        return status != version.Status.Name;
                    }
                    return status != "Inactive";
                }
                return false;
            }
        }
    }
}