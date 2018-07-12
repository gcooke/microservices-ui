using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Models.Monitoring
{
    public class RiskReportResponse
    {
        public int? RowCount { get; set; }
        public string Query { get; set; }
        public string CorrelationId { get; set; }
        public long TimingInMilliseconds { get; set; }
        public bool Successful { get; set; }

        public RiskReportResponse()
        {
            Successful = true;
        }

        public string Timing
        {
            get
            {
                var timespan = TimeSpan.FromMilliseconds(TimingInMilliseconds);
                return $"{timespan.Hours}h {timespan.Minutes}m {timespan.Seconds}s";
            }
        }
    }
}