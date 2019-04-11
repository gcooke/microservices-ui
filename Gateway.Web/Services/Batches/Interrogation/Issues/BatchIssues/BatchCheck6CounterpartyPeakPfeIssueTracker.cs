using System;
using Gateway.Web.Database;
using Gateway.Web.Services.Batches.Interrogation.Attributes;
using Gateway.Web.Services.Batches.Interrogation.Models;

namespace Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues
{
    [AppliesToBatch(Models.Enums.Batches.CounterpartyPfe)]
    public class BatchCheck6CounterpartyPeakPfeIssueTracker : BaseBatchIssueTracker
    {
        public override Models.Issues Identify(GatewayEntities gatewayDb, Batch item)
        {
            var issues = new Models.Issues();
            return issues;
        }

        public override int GetSequence()
        {
            throw new NotImplementedException();
        }
    }
}