using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls.Expressions;

namespace Gateway.Web.Models.Controllers
{
    public class ServersModel
    {
        public ServersModel(ArrayOfGatewayInfo info)
        {
            Root = info;
            ConstructModel();
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
            Status = GetStatus(gatewayInfo);
        }

        public string Node { get; private set; }

        public string Memory { get; private set; }

        public string Disk { get; private set; }

        public string Cpu { get; private set; }

        public int Workers { get; private set; }

        public int Queues { get; private set; }

        public string Status { get; private set; }

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

        private string GetStatus(GatewayInfo gatewayInfo)
        {
            if (gatewayInfo.GatewayNodeServices == null ||
                gatewayInfo.GatewayNodeServices.Any(service => service.CheckID == "serfHealth") == false)
                return string.Empty;

            return gatewayInfo
                .GatewayNodeServices
                .Single(service => service.CheckID == "serfHealth")
                .Status;
        }
    }
}