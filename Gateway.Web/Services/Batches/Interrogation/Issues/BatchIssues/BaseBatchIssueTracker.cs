using System.Collections.Generic;
using Gateway.Web.Database;
using Gateway.Web.Models.Interrogation;
using Gateway.Web.Services.Batches.Interrogation.Models;

namespace Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues
{
    public abstract class BaseBatchIssueTracker : IIssueTracker<Batch>
    {
        public abstract Models.Issues Identify(InterrogationModel model, GatewayEntities gatewayDb, Entities pnrFoDb, Batch item);
        public abstract IEnumerable<string> GetDescriptions();
        public abstract int GetSequence();
    }
}