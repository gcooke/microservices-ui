using System;
using Gateway.Web.Database;
using Gateway.Web.Models.Interrogation;
using Gateway.Web.Services.Batches.Interrogation.Attributes;
using Gateway.Web.Services.Batches.Interrogation.Models;

namespace Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues
{
    [AppliesToBatch(Models.Enums.Batches.CounterpartyPfe)]
    public class BatchCheck6CounterpartyPeakPfeIssueTracker : BaseBatchIssueTracker
    {
        public override Models.Issues Identify(InterrogationModel model, GatewayEntities gatewayDb, Entities pnrFoDb, Batch item)
        {
            var issues = new Models.Issues();

            // Check previous business days Peak PFE values compared to current date
            

            // Check that all Counterparties that have MTM have Peak PFE

            return issues;
        }

        public override int GetSequence()
        {
            return 6;
        }
    }
}