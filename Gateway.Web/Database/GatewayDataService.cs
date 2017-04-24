using System;
using System.Linq;
using Gateway.Web.Models;

namespace Gateway.Web.Database
{
    public class GatewayDataService : IGatewayDataService
    {
        public GatewayDataService()
        {
            //PnRFO_GatewayEntities
        }

        public ControllersModel GetControllers(DateTime start)
        {
            var result = new ControllersModel();

            using (var database = new GatewayEntities())
            {
                // Get stats
                var stats = new ResponseStats(database.spGetResponseStatsAll(start));
                
                foreach (var controller in database.Controllers)
                {
                    var model = controller.ToModel(stats);
                    result.Controllers.Add(model);
                }
            }

            return result;
        }

        public ControllerDetailModel GetControllerInfo(string name)
        {
            using (var database = new GatewayEntities())
            {
                var controller = database.Controllers.FirstOrDefault(c => c.Name == name);
                if (controller == null)
                    return new ControllerDetailModel() { Name = "Unknown controller" };

                var result = new ControllerDetailModel();
                result.Name = name;
                result.Versions.AddRange(controller.Versions.Select(v => v.ToModel()));
                result.Requests.AddRange(GetRecentRequests(database, name, result.RecentRequestsCount));
                return result;
            }
        }

        public ControllerRequestsSummaryModel GetControllerRequestSummary(string name, DateTime start)
        {
            using (var database = new GatewayEntities())
            {
                var controller = database.Controllers.FirstOrDefault(c => c.Name == name);
                if (controller == null)
                    return new ControllerRequestsSummaryModel() { Name = "Unknown controller" };

                // Get recent requests
                var results = database.spGetRequestStats(start, name);
                var result = new ControllerRequestsSummaryModel();
                result.Name = name;
                foreach (var item in results.OrderBy(r=>r.Date))
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

        private Request[] GetRecentRequests(GatewayEntities database, string controller, int count)
        {
            var requests = database.Requests
                                   .Where(r => r.Controller == controller)
                                   .OrderByDescending(r => r.StartUtc)
                                   .Take(count);
            return requests.ToArray();
        }
    }
}