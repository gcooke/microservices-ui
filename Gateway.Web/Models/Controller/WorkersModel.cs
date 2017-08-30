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
                return WorkerInfos.GroupBy(c => c.Service)
                        .Select(c => new ControllerInfo { Controller = c.Key, WorkerInfos = c.ToList() });
            }
        }

        public WorkersModel BuildModel(GatewayInfo gatewayInfo)
        {
            foreach (var version in gatewayInfo.Services.SelectMany(s => s.Versions))
            {
                var workerInfo = new WorkerInfo(version);
                WorkerInfos.Add(workerInfo);
            }

            foreach (var worker in WorkerInfos)
            {
                var found = gatewayInfo
                       .GatewayNodes
                       .Where(n => string.Equals(n.Node, worker.Node, StringComparison.InvariantCultureIgnoreCase))
                       .Select(p => p.Processes)
                       .SelectMany(p => p.Process)
                       .Any(p => p.PID.ToString() == worker.Pid);

                if (!found)
                {
                    worker.Status = "Missing";
                }
            }

            foreach (var gatewayNode in gatewayInfo.GatewayNodes)
            {
                if (gatewayNode.Processes == null || gatewayNode.Processes.Process == null) continue;
                foreach (var process in gatewayNode.Processes.Process)
                {
                    var found = WorkerInfos.Any(w => w.Pid != process.PID.ToString() || w.Node == gatewayNode.Node);

                    if (!found)
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
            }

            return this;
        }
    }
}