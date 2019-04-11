using Gateway.Web.Database;
using Gateway.Web.Services.Batches.Interrogation.Models;

namespace Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues
{
    public abstract class BaseBatchIssueTracker : IIssueTracker<Batch>
    {
        public abstract Models.Issues Identify(GatewayEntities gatewayDb, Batch item);

        public abstract int GetSequence();
    }
}