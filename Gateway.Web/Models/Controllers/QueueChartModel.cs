using System;
using System.Collections.Generic;
using Gateway.Web.Database;

namespace Gateway.Web.Models.Controllers
{
    public class QueueChartModel
    {
        public QueueChartModel()
        {
            Versions = new List<VersionQueueChartModel>();
        }

        public List<VersionQueueChartModel> Versions { get; private set; }
    }

    public class VersionQueueChartModel
    {
        public VersionQueueChartModel(string controller, string version)
        {
            Controller = controller;
            Version = version;
            Label = string.Format("{0} ({1})", Controller, Version);
            Items = new List<QueueCount>();
        }

        public string Label { get; set; }
        public string Controller { get; set; }
        public string Version { get; set; }
        public List<QueueCount> Items { get; private set; }
    }

    public class QueueCount
    {
        public QueueCount(string version, DateTime time, DateTime? lastEnqueue, DateTime? lastDequeue, int count)
        {
            Version = version;
            Time = time;
            LastEnqueue = lastEnqueue;
            LastDequeue = lastDequeue;
            Count = count;
        }

        public string Version { get; set; }
        public DateTime Time { get; set; }
        public DateTime? LastEnqueue { get; set; }
        public DateTime? LastDequeue { get; set; }
        public int Count { get; set; }
    }
}