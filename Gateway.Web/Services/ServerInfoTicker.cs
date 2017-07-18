using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Bagl.Cib.MIT.IoC;
using Gateway.Web.Hubs;
using Gateway.Web.Models.Controllers;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WebGrease.Css.Extensions;

namespace Gateway.Web.Services
{
    public interface IServerInfoTicker
    {
        IHubConnectionContext<object> Clients { get; set; }
        IEnumerable<Server> GetAllServers();
        void UpdateServerInfo(object state);
        void BroadcastServerInfo(Server server);
    }

    public class ServerInfoTicker : IServerInfoTicker
    {
        //Singleton instance
        private static readonly Lazy<ServerInfoTicker> _instance =
            new Lazy<ServerInfoTicker>(
                () => new ServerInfoTicker(GlobalHost.ConnectionManager.GetHubContext<ServerInfoHub>().Clients));

        private readonly IServerInfoService _serverInfoService;
        private readonly ConcurrentDictionary<string, Server> _servers = new ConcurrentDictionary<string, Server>();
        private readonly Timer _timer;
        private readonly object _synchLock = new object();
        private bool _updatingServers;

        public ServerInfoTicker(
            IHubConnectionContext<dynamic> clients
            //TODO: Inject IServerInfoService - issue with different resolvers.
            //, IServerInfoService serverInfoService
            )
        {
            Clients = clients;

            var resolver = DependencyResolver.Current;

            var serverInfoService = resolver.GetService(typeof(IServerInfoService));

            if (serverInfoService == null)
                throw new ArgumentException(nameof(serverInfoService));

            _serverInfoService = (IServerInfoService)serverInfoService;

            var information = resolver.GetService(typeof(ISystemInformation));
            string intervalMsConfigValue =
                (information == null) ? "1000" : ((ISystemInformation)information).GetSetting("TickerIntervalMs");

            double intervalMs;
            if (!double.TryParse(intervalMsConfigValue, out intervalMs))
                intervalMs = 1000;
            
            var updateInterval = TimeSpan.FromMilliseconds(intervalMs);
            var startInterval = TimeSpan.FromMilliseconds(2000);
            _timer = new Timer(UpdateServerInfo, null, startInterval, updateInterval);
        }

        public static ServerInfoTicker Instance
        {
            get { return _instance.Value; }
        }

        public IHubConnectionContext<dynamic> Clients { get; set; }

        public IEnumerable<Server> GetAllServers()
        {
            return _servers.Values;
        }

        public void UpdateServerInfo(object state)
        {
            if (_updatingServers) return;
            lock (_synchLock)
            {
                if (_updatingServers) return;
                _updatingServers = true;

                // Retrieve servers
                var servers = _serverInfoService.GetServers();

                // Update server info
                _servers.Clear();
                servers.ForEach(server => _servers.TryAdd(server.Node, server));

                // Broadcast server info
                foreach (var server in _servers.Values)
                {
                    BroadcastServerInfo(server);
                }
                _updatingServers = false;
            }
        }

        public void BroadcastServerInfo(Server server)
        {
            Clients.All.updateServerInfo(server);
        }
    }
}