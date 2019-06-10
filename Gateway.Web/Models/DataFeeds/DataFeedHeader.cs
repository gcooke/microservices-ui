using System;
using System.Collections.Generic;

namespace Gateway.Web.Models.DataFeeds
{
    public class DataFeedHeader
    {
        public long Id { get; set; }
        public string DataFeedName { get; set; }
        public decimal TotalTimeTakenInSeconds { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Ended { get; set; }
        public string Message { get; set; }
        public string DataFeedStatus { get; set; }
        public string DataFeedType { get; set; }

        public List<DataFeedDetail> DataFeedDetail { get; set; }
    }
}