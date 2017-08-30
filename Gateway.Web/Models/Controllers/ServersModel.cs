using System;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Models.Controllers
{
    public class ServersModel
    {
        public ServersModel(GatewayInfo info)
        {
            Workers = new List<WorkerInfo>();

            Root = info;
            ConstructModel();
        }

        public ServersModel()
        {
            Servers = new List<Server>();
        }

        public GatewayInfo Root { get; }

        public IEnumerable<Server> Servers { get; private set; }

        private List<WorkerInfo> Workers { get; set; }

        public void ConstructModel()
        {
            BuildWorkers();

            Servers = Root
                .GatewayNodes
                .Select(gatewayNode => new Server(gatewayNode).SetWorkers(Workers))
                .ToList();
        }

        public void BuildWorkers()
        {
            if (Root.Services == null) return;
            Workers = Root.Services.SelectMany(s => s.Versions).Select(c => new WorkerInfo(c)).ToList();
        }
    }

    public class Server
    {
        private List<WorkerInfo> _workers;

        public Server(GatewayInfoGatewayNode gatewayInfo)
        {
            Node = gatewayInfo.Node;
            Queues = GetQueueSize(gatewayInfo.Queues);
            if (gatewayInfo.PerformanceCounters != null)
            {
                Cpu = string.Format("{0} %", gatewayInfo.PerformanceCounters.CpuUsage);
                Memory = string.Format("{0} MBytes", gatewayInfo.PerformanceCounters.MemUsage);
            }
        }

        public string Node { get; }

        public string Memory { get; private set; }

        public string Disk { get; private set; }

        public string Cpu { get; private set; }

        public int Workers
        {
            get { return _workers.Count; }
        }

        public int Queues { get; private set; }

        public string Status { get; private set; }

        public string Output { get; private set; }

        public Server SetWorkers(List<WorkerInfo> workers)
        {
            _workers = workers.Where(c => c.Node == Node).ToList();
            GetStatusAndOutput();
            return this;
        }

        private int GetQueueSize(GatewayInfoGatewayNodeQueues gatewayInfo)
        {
            if (gatewayInfo== null || gatewayInfo.ControllerQueueInfos == null) return 0;

            return gatewayInfo
                .ControllerQueueInfos
                .Sum(queue => queue.Length);
        }

        private void GetStatusAndOutput()
        {
            if (_workers == null)
            {
                Status = "warning";
                Output = "Gateway did not return service info";
            }

            if (_workers.Any() == false)
            {
                Status = "success";
                Output = "No workers";
            }

            var allItemsPassing = _workers
                .All(s => s.Status.Equals("Passing", StringComparison.InvariantCultureIgnoreCase));

            Status = allItemsPassing ? "success" : "warning";
            Output = "Healthy";
        }
    }
}