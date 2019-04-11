using Gateway.Web.Database;

namespace Gateway.Web.Services.Batches.Interrogation.Issues
{
    public interface IIssueTracker<in T>
    {
        Models.Issues Identify(GatewayEntities gatewayDb, Entities pnrFoDb, T item);
        int GetSequence();
    }
}
