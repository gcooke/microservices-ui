using Gateway.Web.Database;
using Gateway.Web.Models.Interrogation;

namespace Gateway.Web.Services.Batches.Interrogation.Issues
{
    public interface IIssueTracker<in T>
    {
        Models.Issues Identify(InterrogationModel model, GatewayEntities gatewayDb, Entities pnrFoDb, T item);
        int GetSequence();
    }
}
