using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Models.Controllers;

namespace Gateway.Web.Models.Controller
{
    public class WorkersModel : BaseControllerModel
    {
        public WorkersModel(string name) : base(name)
        {
            WorkerInfos = new List<WorkerInfo>();
        }

        public List<WorkerInfo> WorkerInfos { get; set; }

        public IEnumerable<ControllerInfo> Workers
        {
            get
            {
                return WorkerInfos
                    .GroupBy(c => c.Service)
                        .Select(c => new ControllerInfo { Controller = c.Key, WorkerInfos = c.ToList() });
            }
        }

        public WorkersModel BuildModel(GatewayInfo gatewayInfo)
        {
            Func<string, string, bool> processIsAlive = (node, pid) =>
            {
                return gatewayInfo
                    .GatewayNodes
                    .Where(n => string.Equals(n.Node, node, StringComparison.InvariantCultureIgnoreCase))
                    .Select(p => p.Processes)
                    .SelectMany(p => p.Process)
                    .Any(n => string.Equals(n.PID.ToString(), pid, StringComparison.InvariantCultureIgnoreCase));
            };

            foreach (var version in gatewayInfo.Services.SelectMany(s => s.Versions))
            {
                var workerInfo = new WorkerInfo(version);
                var isAlive = processIsAlive(workerInfo.Node, workerInfo.Pid);
                workerInfo.Status = isAlive ? workerInfo.Status : "Missing";
                WorkerInfos.Add(workerInfo);
            }

            foreach (var gatewayNode in gatewayInfo.GatewayNodes)
            {
                if (gatewayNode.Processes == null ||
                    gatewayNode.Processes.Process == null) continue;

                var validProcessIds = WorkerInfos
                    .Where(n => string.Equals(n.Node, gatewayNode.Node, StringComparison.InvariantCultureIgnoreCase))
                    .Select(p => p.Pid);

                var unknownProcesses = gatewayNode
                                        .Processes
                                        .Process
                                        .Where(p => !validProcessIds.Contains(p.PID.ToString()))
                                        .ToList();

                foreach (var process in unknownProcesses)
                {
                    var workerInfo = new WorkerInfo
                    {
                        Id = string.Concat(process.Name, "|", process.PID),
                        Node = gatewayNode.Node,
                        Service = string.Concat(process.Name, "|", process.PID),
                        Status = "Orphaned"
                    };

                    WorkerInfos.Add(workerInfo);
                }
            }

            return this;
        }
    }
}