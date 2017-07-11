using System.Collections.Generic;
using Gateway.Web.Models.Controllers;

namespace Gateway.Web.Services
{
    public class ServerInfoService : IServerInfoService
    {
        private readonly IGatewayService _gatewayService;

        public ServerInfoService(IGatewayService gatewayService)
        {
            _gatewayService = gatewayService;
        }

        public IEnumerable<Server> GetServers()
        {
            var serversModel = _gatewayService.GetServers();

            return serversModel.Servers;
        }
    }

    public interface IServerInfoService
    {
        IEnumerable<Server> GetServers();
    }
}