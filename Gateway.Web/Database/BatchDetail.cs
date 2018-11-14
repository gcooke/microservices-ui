using System;
using System.Collections.Generic;

namespace Gateway.Web.Database
{
    public class BatchDetail
    {
        public string LegalEntity { get; set; }
        public DateTime ValuationDate { get; set; }
        public IList<BatchExecution> BatchExecutions { get; set; }

        public BatchDetail()
        {
            BatchExecutions = new List<BatchExecution>();
        }

        public string BatchName => LegalEntity
            .Replace("-", " - ")
            .Replace("_", " ");
    }
}