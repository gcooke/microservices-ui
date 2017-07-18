using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        }

        public ArrayOfGatewayInfo Root { get; private set; }

        public IEnumerable<Server> Servers { get; private set; }

        public void ConstructModel()
        {
            Servers = Root
                .GatewayInfo
                .Select(gatewayInfo =>
                        new Server(gatewayInfo)
                            .BuildPerformanceCounters()
                )
                .ToList();

            PerformanceCounter.CloseSharedResources();
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
        }

        public Server BuildPerformanceCounters()
        {
            Cpu = CounterValue("processor", "% Processor Time", "_Total");
            Memory = CounterValue("Memory", "% Committed Bytes In Use");
            Disk = "err";
            return this;
        }

        private string CounterValue(string categoryName, string counterName, string instanceName = "")
        {
            PerformanceCounter counter = null;
            try
            {

                counter = instanceName == string.Empty ?
                    new PerformanceCounter(categoryName, counterName, null, Node) :
                    new PerformanceCounter(categoryName, counterName, instanceName, Node);
                return counter.NextValue().ToString("4f");
            }
            catch (Exception)
            {
                return "Err";
            }
            finally
            {
                if (counter != null)
                {
                    counter.Close();
                    counter.Dispose();
                }
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
            if (gatewayInfo.GatewayNodeServices == null ||
                gatewayInfo.GatewayNodeServices.Any(service => service.CheckID == "serfHealth") == false)
                return;

            Status = gatewayInfo
                .GatewayNodeServices
                .Single(service => service.CheckID == "serfHealth")
                .Status;

            Output = gatewayInfo
                .GatewayNodeServices
                .Single(service => service.CheckID == "serfHealth")
                .Output;
        }
    }
}