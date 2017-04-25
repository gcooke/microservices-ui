using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Models.Controller;

namespace Gateway.Web.Database
{
    public class GatewayDataService : IGatewayDataService
    {
        public GatewayDataService()
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

        public RequestModel GetRequestDetails(string name, string correlationId)
        {
            var result = new RequestModel(name);
            var id = Guid.Parse(correlationId);
            using (var database = new GatewayEntities())
            {
                foreach (var item in database.Payloads.Where(p => p.CorrelationId == id))
                {
                    result.Items.Add(new PayloadModel(item));
                }
            }
            return result;
        }

        //private Request[] GetRecentRequests(GatewayEntities database, string controller, int count)
        //{
        //    var requests = database.Requests
        //                           .Where(r => r.Controller == controller)
        //                           .OrderByDescending(r => r.StartUtc)
        //                           .Take(count);
        //    return requests.ToArray();
        //}
    }
}