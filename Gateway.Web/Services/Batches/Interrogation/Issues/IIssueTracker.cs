using Gateway.Web.Database;

namespace Gateway.Web.Services.Batches.Interrogation.Issues
{
    public interface IIssueTracker<in T>
    {
        Models.Issues Identify(GatewayEntities gatewayDb, T item);
        int GetSequence();
    }
}
