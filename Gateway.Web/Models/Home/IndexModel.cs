using System;
using Gateway.Web.Models.Controllers;

namespace Gateway.Web.Models.Home
{
    public class IndexModel
    {
        public IndexModel()
        {
            HistoryStartTime = DateTime.Now.AddDays(-1);
        }

        public DateTime HistoryStartTime { get; set; }

        public QueueChartModel Queues { get; set; }
    }
}