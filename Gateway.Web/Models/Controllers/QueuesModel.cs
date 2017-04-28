using System;
using System.Collections.Generic;
using Gateway.Web.Models.Controller;

namespace Gateway.Web.Models.Controllers
{
    public class QueuesModel
    {
        public QueuesModel()
        {
            HistoryStartTime = DateTime.Now.AddDays(-1);
            Current = new List<QueueModel>();
        }

        public DateTime HistoryStartTime { get; set; }

        public QueueChartModel Queues { get; set; }

        public List<QueueModel> Current { get; set; }
    }
}