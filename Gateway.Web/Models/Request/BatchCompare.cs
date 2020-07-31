using System;

namespace Gateway.Web.Models.Request
{
    public class BatchCompare
    {
        public Guid CorrelationId { get; set; }

        public string Controller { get; set; }

        public Summary CurrentSummary { get; set; }
        public Summary OldSummary { get; set; }
    }
}