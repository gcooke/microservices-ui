using System;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Models.Controllers
{
    public class Controller
    {
        public Controller()
        {

            Workers = new List<Worker>();
        }

        public List<Worker> Workers { get; set; }

        public int Errors
        {
            get { return Workers.Count(c => !c.StateOk); }
        }

        public int Count
        {
            get { return Workers.Count(); }
        }

        public string Name { get; set; }

        public void Add(string node, int pid, string version, string status, Guid correlationId)
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
        public int ProcessId { get; set; }
        public Guid CorrelationId { get; set; }
        public string Version { get; set; }

        public bool Healthy
        {
            get { return Status == "passing"; }
        }

        public bool Registered
        {
            get { return Status == "passing"; }
        }

        public string Status { get; set; }

        public bool StateOk
        {
            get { return Status == "passing"; }
        }
    }
}