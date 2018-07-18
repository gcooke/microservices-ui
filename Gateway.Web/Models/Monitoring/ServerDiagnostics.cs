using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Models.Monitoring
{
    public class ServerDiagnostics
    {
        public string ServerName { get; set; }
        public DateTime Timestamp { get; set; }
        public float CpuUtilization { get; set; }
        public float MemoryUtilization { get; set; }
        public IList<WorkerStats> Workers { get; set; }
        public IList<RequestStats> Requests { get; set; }
        public bool Available { get; set; }
        public int MemoryAvailableGigabytes => (int)(MemoryUtilization / 1000);

        public int BusyWorkerCount
        {
            get { return Workers.Sum(x => x.WorkerInProgressCount); }
        }

        public int TotalWorkerCount
        {
            get { return Workers.Sum(x => x.WorkerCount); }
        }

        public int CpuUsage => (int) CpuUtilization;
    }
}