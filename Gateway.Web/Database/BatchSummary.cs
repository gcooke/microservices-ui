using System;
using Gateway.Web.Utils;

namespace Gateway.Web.Database
{
    public class BatchSummary
    {
        public string LegalEntity { get; set; }
        public string Controller { get; set; }
        public string ControllerVersion { get; set; }
        public DateTime? ValuationDate { get; set; }
        public int ExecutionCount { get; set; }
        public DateTime StartTime { get; set; }
        public long Duration { get; set; }
        public int TradeCount { get; set; }
        public int FatalErrorCount { get; set; }
        public int TotalErrorCount { get; set; }
        public Guid RequestCorrelationId { get; set; }

        public string TimeTakenMs => TimeSpan.FromMilliseconds(Duration).Humanize();

        public string GetBatchName(string site)
        {
            return LegalEntity
                .Replace(site, "")
                .Replace(site.ToUpper(), "")
                .Replace("-", "")
                .Trim();
        }
    }
}