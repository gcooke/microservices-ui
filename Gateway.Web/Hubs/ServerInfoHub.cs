using System;
using System.Collections.Generic;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Services;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Gateway.Web.Hubs
{
    [HubName("serverInfoTicker")]
    public class ServerInfoHub : Hub
    {
        private readonly ServerInfoTicker _serverInfoTicker;

        public ServerInfoHub() :
            this(ServerInfoTicker.Instance)
        {

        }

        public ServerInfoHub(ServerInfoTicker serverInfoTicker)
        {
            if (serverInfoTicker == null)
            {
                throw new ArgumentNullException(nameof(serverInfoTicker));
            }
            _serverInfoTicker = serverInfoTicker;
        }

        public IEnumerable<Server> GetAllServers()
        {
            return _serverInfoTicker.GetAllServers();
        }
    }
}