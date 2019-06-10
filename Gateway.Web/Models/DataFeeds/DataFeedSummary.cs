using Gateway.Web.Models.Controller;
using System;

namespace Gateway.Web.Models.DataFeeds
{
    public class DataFeedSummary : BaseControllerModel
    {
        public DataFeedSummary(string name) : base(name)
        {
            HistoryStartTime = DateTime.Today.AddDays(-7);
        }

        public DateTime HistoryStartTime { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string Duration { get; set; }
    }
}