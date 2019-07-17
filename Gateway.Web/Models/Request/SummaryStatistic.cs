using System;

namespace Gateway.Web.Models.Request
{
    public class SummaryStatistic
    {
        public string Controller { get; set; }

        public int CallCount { get; set; }

        public TimeSpan TotalQueueTime { get; set; }

        public TimeSpan TotalProcessingTime { get; set; }
    }
}