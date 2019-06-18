using System;
using System.Collections.Generic;

namespace Gateway.Web.Models.DataFeeds
{
    public class DataFeedHeader
    {
        public string DataFeedName { get; set; }

        public DateTime? Started { get; set; }
        public DateTime? Ended { get; set; }
        public string Message { get; set; }
        public string DataFeedStatus { get; set; }
        public string DataFeedType { get; set; }
        public int DataFeedTypeId { get; set; }
        public List<DataFeedDetail> DataFeedDetail { get; set; }
    }
}