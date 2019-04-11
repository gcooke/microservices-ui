using System;
using System.Collections.Generic;

namespace Gateway.Web.Services.Batches.Interrogation.Models
{
    public class Batch
    {
        public string BatchType { get; set; }
        public string TradeSource { get; set; }
        public string Site { get; set; }
        public string TradeSourceType { get; set; }
        public IEnumerable<DateTime> ExpectedOccurrences { get; set; }
        public IList<BatchRun> ActualOccurrences { get; set; }

        public Batch()
        {
            ExpectedOccurrences = new List<DateTime>();
            ActualOccurrences = new List<BatchRun>();
        }

        public override string ToString()
        {
            return $"{BatchType} - {TradeSource} [{Site}] ({TradeSourceType})";
        }
    }
}