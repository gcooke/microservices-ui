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
                .Select(gatewayInfo =>
                        new Server(gatewayInfo)
                            .BuildPerformanceCounters()
                )
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
        }

        public Server BuildPerformanceCounters()
        {
            var processorTime = CounterValue("Processor", "% Processor Time", "_Total");
            var memUsage = CounterValue("Memory", "Available MBytes");

            Cpu = string.Format("{0} % Usage", processorTime);
            Memory = string.Format("{0} RAM Avail.", memUsage);

            PerformanceCounter.CloseSharedResources();

            return this;
        }

        private object CounterValue(string categoryName, string counterName, string instanceName = "")
        {
            PerformanceCounter counter = null;
            try
            {
                counter = instanceName == string.Empty ?
                    new PerformanceCounter(categoryName, counterName, Node, null) :
                    new PerformanceCounter(categoryName, counterName, Node, instanceName);

                // will always start at 0
                dynamic firstValue = counter.NextValue();
                Thread.Sleep(1000);
                // now matches task manager reading
                dynamic secondValue = counter.NextValue();

                return secondValue;
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