using System;
using System.Collections.Generic;
using Gateway.Web.Models.Controller;
using QueueChartModel = Gateway.Web.Models.Controllers.QueueChartModel;

namespace Gateway.Web.Models.Home
{
    public class IndexModel
    {
        public IndexModel()
        {
            HistoryStartTime = DateTime.Now.AddDays(-1);
            Current = new List<QueueModel>();
        }

        public DateTime HistoryStartTime { get; set; }

        public QueueChartModel Queues { get; set; }

        public List<QueueModel> Current { get; set; }
    }
}