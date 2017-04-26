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
        public VersionQueueChartModel(spGetQueueCountsAll_Result lastRecord)
        {
            Controller = lastRecord.Controller;
            Version = lastRecord.Version;
            LastEnqueue = lastRecord.LastEnqueue;
            LastDequeue = lastRecord.LastDequeue;
            Label = string.Format("{0} ({1})", Controller, Version);
            LastCount = lastRecord.ItemCount.Value;
            Items = new List<QueueCount>();
        }

        public string Label { get; set; }
        public string Controller { get; set; }
        public string Version { get; set; }
        public DateTime? LastEnqueue { get; set; }
        public DateTime? LastDequeue { get; set; }
        public int LastCount { get; set; }
        public List<QueueCount> Items { get; private set; }
    }

    public class QueueCount
    {
        public QueueCount(DateTime time, int count)
        {
            Time = time;
            Count = count;
        }

        public DateTime Time { get; set; }
        public int Count { get; set; }
    }
}