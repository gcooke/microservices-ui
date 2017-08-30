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

        public WorkersModel() : this(string.Empty)
        {
        }

        public List<WorkerInfo> WorkerInfos { get; set; }

        public IEnumerable<ControllerInfo> Workers
        {
            get
            {
                return WorkerInfos
                    .Where(c => string.Equals(c.Controller, Name, StringComparison.InvariantCultureIgnoreCase) || string.IsNullOrEmpty(Name))
                    .GroupBy(c => c.Service)
                        .Select(c =>
                        new ControllerInfo { Controller = c.Key, WorkerInfos = c.ToList() });
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
                workerInfo.Status = isAlive ? workerInfo.Status : "missing";
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

                //D:\Services\Redstone\Gateway\1.0.833\Bagl.Cib.MSF.ControllerHost.exe 
                //"D:\Services\Redstone\Gateway\1.0.833\Bagl.Cib.MSF.ControllerHost.exe"
                //-controller catalogue -version 1.1.56 -environment UAT -id 91a753e4-7dc6-4a01-a4bb-69d7e4a08a18 
                foreach (var process in unknownProcesses)
                {
                    var serviceName = GetServiceName(process);
                    var workerInfo = new WorkerInfo
                    {
                        Service = serviceName,
                        Id = string.Concat(serviceName, "|", process.PID),
                        Node = gatewayNode.Node.ToUpper(),
                        Output = "Active",
                        Status = "orphaned"
                    };

                    WorkerInfos.Add(workerInfo);
                }
            }

            return this;
        }

        private string GetServiceName(GatewayInfoGatewayNodeProcessesProcess process)
        {
            try
            {
                //dirty code to extract controller details from args
                var controller = process.Args.Substring(process.Args.IndexOf("-controller "),
                    process.Args.IndexOf("-version ") - process.Args.IndexOf("-controller ")).Replace("-controller", string.Empty).Trim();

                var version = process.Args.Substring(process.Args.IndexOf("-version "),
                    process.Args.IndexOf("-environment ") - process.Args.IndexOf("-version ")).Replace("-version", string.Empty).Trim();

                return string.Concat(controller, "/", version);

            }
            catch (Exception)
            {
                return string.Concat(process.Name, "/", "0.0.0");
            }
        }
    }
}