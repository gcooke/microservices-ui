using System;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Models.Controllers
{
    public class ManageWorkersModel
    {
        public ManageWorkersModel()
        {
            Controllers = new List<Controller>();
            Add("marketdata", "JHBPSM020000758", "49056", "1.0.80", "passing", Guid.NewGuid());
            Add("marketdata", "JHBPSM020000758", "49056", "1.0.80", "missing", Guid.NewGuid());
            Add("marketdata", "JHBPSM020000758", "49056", "1.0.80", "orphan", Guid.NewGuid());
            Add("marketdata", "JHBPSM020000758", "49056", "1.0.80", "passing", Guid.NewGuid());
            Add("pricing", "JHBPSM020000757", "49056", "1.1.630", "orphan", Guid.NewGuid());
            Add("pricing", "JHBPSM020000758", "49056", "1.1.630", "missing", Guid.NewGuid());
            Add("pricing", "JHBPSM020000757", "49056", "1.1.630", "passing", Guid.NewGuid());
            Add("pricing", "JHBPSM020000758", "49056", "1.1.630", "passing", Guid.NewGuid());
        }

        public void Add(string controllerName, string node, string pid, string version, string status, Guid correlationId)
        {
            var controller = Controllers.FirstOrDefault(e => e.Name == controllerName);
            if (controller == null)
            {
                controller = new Controller
                {
                    Name = controllerName,
                };

                Controllers.Add(controller);
            }

            controller.Add(node, pid, version, status, correlationId);
        }

        public List<Controller> Controllers { get; }
    }

    public class Controller
    {
        public Controller()
        {

            Workers = new List<Worker>();
        }
        public List<Worker> Workers { get; set; }
        public int Errors { get; set; }
        public int Count { get; set; }
        public string Name { get; set; }

        public void Add(string node, string pid, string version, string status, Guid correlationId)
        {
            var worker = new Worker
            {
                Node = node,
                ProcessId = pid,
                Version = version,
                Status = status,
                CorrelationId = correlationId
            };

            Workers.Add(worker);
        }
    }

    public class Worker
    {
        public string Node { get; set; }
        public string ProcessId { get; set; }
        public Guid CorrelationId { get; set; }
        public string Version { get; set; }

        public bool Exists
        {
            get { return Status == "orphaned"; }
        }
        public bool Alive
        {
            get { return Status == "passing"; }
        }
        public string Status { get; set; }

        public bool State
        {
            get { return Status == "passing"; }
        }
    }
}