using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Models.Controller
{
    public class QueuesModel : BaseControllerModel
    {
        public QueuesModel(string name) : base(name)
        {
            HistoryStartTime = DateTime.Now.AddDays(-1);
        }

        public DateTime HistoryStartTime { get; set; }

        public QueueChartModel Queues { get; set; }
    }
}