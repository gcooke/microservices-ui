using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
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
        void LoadServers();
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

        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(20);

        private readonly object _updateServerLock = new object();

        private volatile bool _updatingServers;

        public ServerInfoTicker(
            IHubConnectionContext<dynamic> clients
            //TODO: Inject IServerInfoService - issue with different resolvers.
            //, IServerInfoService serverInfoService
            )
        {
            Clients = clients;
            var resolver = DependencyResolver.Current;
            var serverInfoService = (IServerInfoService) resolver.GetService(typeof (IServerInfoService));
            _serverInfoService = serverInfoService;
            _timer = new Timer(UpdateServerInfo, null, _updateInterval, _updateInterval);
            LoadServers();
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

        public void LoadServers()
        {
            _servers.Clear();
            var servers = _serverInfoService.GetServers();
            servers.ForEach(server => _servers.TryAdd(server.Node, server));
        }

        public void UpdateServerInfo(object state)
        {
            lock (_updateServerLock)
            {
                if (!_updatingServers)
                {
                    _updatingServers = true;

                    LoadServers();

                    foreach (var server in _servers.Values)
                    {
                        BroadcastServerInfo(server);
                    }

                    _updatingServers = false;
                }
            }
        }

        public void BroadcastServerInfo(Server server)
        {
            Clients.All.updateServerInfo(server);
        }
    }
}