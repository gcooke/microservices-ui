using System;

namespace Gateway.Web.Models.DataFeeds
{
    public class DataFeedDashboardItem
    {
        public DataFeedHeader DataFeedHeader { get; set; }

        public DateTime RunDate { get; set; }

        public int DateFeedTypeId { get; set; }
    }
}