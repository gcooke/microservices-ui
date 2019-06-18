using System;

namespace Gateway.Web.Models.DataFeeds
{
    public class DataFeedDetail
    {
        public string SourceName { get; set; }
        public int SourceTotalRows { get; set; }
        public string DestinationName { get; set; }
        public int DestinationTotalRows { get; set; }
        public decimal TimeTakenInSeconds { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Ended { get; set; }
        public string Message { get; set; }
        public bool IsSuccessful { get; set; }
    }
}