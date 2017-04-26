using System;

namespace Gateway.Web.Models.Controllers
{
    public class QueuesModel
    {
        public QueuesModel()
        {
            HistoryStartTime = DateTime.Now.AddDays(-1);
        }

        public DateTime HistoryStartTime { get; set; }

        public QueueChartModel Queues { get; set; }
    }
}