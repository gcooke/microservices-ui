using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Gateway.Web.Models.Controllers
{
    public class ServersModel
    {
        public ServersModel(ArrayOfGatewayInfo info)
        {
            Root = info;
            ConstructModel();
        }
        public ServersModel()
        {
            Servers = new List<Server>();
        }

        public ArrayOfGatewayInfo Root { get; private set; }

        public IEnumerable<Server> Servers { get; private set; }

        public void ConstructModel()
        {
            Servers = Root
                .GatewayInfo
                .Select(gatewayInfo => new Server(gatewayInfo))
                .ToList();
        }
    }

    public class Server
    {
        public Server(GatewayInfo gatewayInfo)
        {
            Node = gatewayInfo.GatewayNode.Node;
            Workers = GetWorkerCount(gatewayInfo);
            Queues = GetQueueSize(gatewayInfo);
            GetStatusAndOutput(gatewayInfo);

            if (gatewayInfo.PerformanceCounters != null)
            {
                Cpu = string.Format("{0} %", gatewayInfo.PerformanceCounters.CpuUsage);
                Memory = string.Format("{0} MBytes", gatewayInfo.PerformanceCounters.MemUsage);
            }
        }

        public string Node { get; private set; }

        public string Memory { get; private set; }

        public string Disk { get; private set; }

        public string Cpu { get; private set; }

        public int Workers { get; private set; }

        public int Queues { get; private set; }

        public string Status { get; private set; }

        public string Output { get; private set; }

        private int GetQueueSize(GatewayInfo gatewayInfo)
        {
            if (gatewayInfo.ControllerProxyStates == null) return 0;

            return gatewayInfo
                  .ControllerProxyStates
                  .Where(queue => queue.Queue != null)
                  .Select(queue => queue.Queue)
                  .Where(queueSize => queueSize.QueueSizes != null)
                  .SelectMany(queueSize => queueSize.QueueSizes)
                  .Sum(queue => queue.Value);
        }

        private int GetWorkerCount(GatewayInfo gatewayInfo)
        {
            if (gatewayInfo.ControllerProxyStates == null) return 0;

            return gatewayInfo
                .ControllerProxyStates
                .Where(proxy => proxy.WorkerState != null)
                .Select(proxy => proxy.WorkerState)
                .Where(proxy => proxy.LiveWorkers != null)
                .SelectMany(proxy => proxy.LiveWorkers)
                .Count();
        }

        private void GetStatusAndOutput(GatewayInfo gatewayInfo)
        {
            if (gatewayInfo.GatewayNodeServices == null)
            {
                Status = "warning";
                Output = "Gateway did not return service info";
                return;
            }

            var allItemsPassing = gatewayInfo
                .GatewayNodeServices
                .All(s => s.Status.Equals("Passing", StringComparison.InvariantCultureIgnoreCase));

            Status = allItemsPassing ? "success" : "danger";

            if (allItemsPassing)
            {
                var serverHealthInfoItem = gatewayInfo
                    .GatewayNodeServices
                    .SingleOrDefault(service => service.CheckID == "serfHealth");

                Output = serverHealthInfoItem == null ? "Unknown" : serverHealthInfoItem.Output;
            }
            else
            {
                var failingItemsCount =
                      gatewayInfo
                    .GatewayNodeServices
                    .Count(s => !s.Status.Equals("Passing", StringComparison.InvariantCultureIgnoreCase));

                Output = string.Format("{0} failing", failingItemsCount);
            }
        }
    }
}