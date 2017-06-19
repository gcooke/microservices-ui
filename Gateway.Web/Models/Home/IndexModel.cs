using System;
using System.Collections.Generic;
using Gateway.Web.Models.Controller;

namespace Gateway.Web.Models.Home
{
    public class IndexModel
    {
        public IndexModel()
        {
            HistoryStartTime = DateTime.Now.AddDays(-1);
            Requests = new List<HistoryItem>();
        }

        public DateTime HistoryStartTime { get; set; }

        public List<HistoryItem> Requests { get; private set; }
    }
}