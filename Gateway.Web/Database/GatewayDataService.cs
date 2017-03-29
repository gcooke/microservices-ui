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

        public Catalogue GetCatalogue()
        {
            var result = new Catalogue();

            using (var database = new GatewayEntities())
            {
                var recent = GetRequestSummary(database);
                foreach (var controller in database.Controllers)
                {
                    result.Controllers.Add(controller.ToModel(recent));
                }
            }

            return result;
        }

        private RecentRequests GetRequestSummary(GatewayEntities database)
        {
            var yesterday = DateTime.Now.AddDays(-1);
            var requests = database.Requests
                                   .Where(r => r.StartUtc > yesterday);
            return new RecentRequests(requests);
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