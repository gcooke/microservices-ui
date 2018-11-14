using System.Collections.Generic;

namespace Gateway.Web.Database
{
    public class BatchExecution
    {
        public BatchSummary Summary { get; set; }
        public IList<BatchIssues> Issues { get; set; }
        public long BatchStatId { get; set; }

        public BatchExecution()
        {
            Issues = new List<BatchIssues>();
        }
    }
}