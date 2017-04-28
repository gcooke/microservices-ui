using System;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Models.Controller
{
    public class QueueChartModel : BaseControllerModel
    {
        public QueueChartModel(string name) : base(name)
        {
            Versions = new List<VersionQueueChartModel>();
        }

        public List<VersionQueueChartModel> Versions { get; private set; }

        public IEnumerable<QueueCount> AllItems
        {
            get { return GetItems().OrderBy(i => i.Time); }
        }

        private IEnumerable<QueueCount> GetItems()
        {
            foreach (var item in Versions)
                foreach (var x in item.Items)
                    yield return x;
        }
    }

    public class VersionQueueChartModel
    {
        public VersionQueueChartModel(string version)
        {
            Version = version;
            Items = new List<QueueCount>();
        }

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