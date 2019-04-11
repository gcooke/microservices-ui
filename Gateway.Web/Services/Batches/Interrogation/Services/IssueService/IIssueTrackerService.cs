using System.Collections.Generic;
using Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues;

namespace Gateway.Web.Services.Batches.Interrogation.Services.IssueService
{
    public interface IIssueTrackerService
    {
        IEnumerable<BaseBatchIssueTracker> GetIssueTrackersForBatch(string batchName);
    }
}
