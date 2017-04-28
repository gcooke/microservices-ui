using System.Collections.Generic;
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
    }
}