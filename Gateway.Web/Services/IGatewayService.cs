using System.Collections.Generic;
using System.Xml.Linq;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Controllers;

namespace Gateway.Web.Services
{
    public interface IGatewayService
    {
        ServersModel GetServers();
        WorkersModel GetWorkers();
        WorkersModel GetWorkers(string controller);
        IEnumerable<QueueModel> GetCurrentQueues(string controller);
        IEnumerable<QueueModel> GetCurrentQueues();
        XElement[] GetReport(string url);
        void UpdateControllerVersionStatuses(string controller, Dictionary<string, string> versionStatusUpdates);
        void MarkVersionsForDelete(string controller, List<string> versionsMarkedForDelete);
        void RefreshCatalogueForAllGateways();
    }
}