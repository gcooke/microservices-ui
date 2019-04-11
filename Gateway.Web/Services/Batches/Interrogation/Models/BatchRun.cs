using System;

namespace Gateway.Web.Services.Batches.Interrogation.Models
{
    public class BatchRun
    {
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string CurrentStatus { get; set; }
        public DateTime? ValuationDate { get; set; }
        public Guid? CorrelationId { get; set; }

    }
}