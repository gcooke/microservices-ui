using System;
using System.Collections.Generic;

namespace Gateway.Web.Models.DataFeeds
{
    public class DataFeedDashboard
    {
        public DataFeedDashboard()
        {
            DataFeeds = new List<DataFeedSummary>();
            BusinessDate = DateTime.Now;
        }

        public List<DataFeedSummary> DataFeeds;
        public DateTime BusinessDate { get; set; }
    }
}