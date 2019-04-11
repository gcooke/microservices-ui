using Gateway.Web.Database;
using Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues;
using Gateway.Web.Services.Batches.Interrogation.Models;

namespace Gateway.Web.Services.Batches.Interrogation.Issues
{
    public interface IIssueTracker<in T>
    {
        Models.Issues Identify(GatewayEntities gatewayDb, Entities pnrFoDb, T item, BatchRun run);
        int GetSequence();
        void SetContext(BatchInterrogationContext context);
    }
}
