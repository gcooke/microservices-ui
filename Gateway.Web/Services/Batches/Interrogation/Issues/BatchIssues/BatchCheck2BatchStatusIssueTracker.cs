using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Database;
using Gateway.Web.Models.Interrogation;
using Gateway.Web.Services.Batches.Interrogation.Attributes;
using Gateway.Web.Services.Batches.Interrogation.Models;
using Gateway.Web.Services.Batches.Interrogation.Models.Builders;
using Gateway.Web.Services.Batches.Interrogation.Models.Enums;

namespace Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues
{
    [AppliesToBatch(Models.Enums.Batches.All)]
    public class BatchCheck2BatchStatusIssueTracker : BaseBatchIssueTracker
    {
        public override IEnumerable<string> GetDescriptions()
        {
            yield return "Check that the batch has completed";
        }

        public override Models.Issues Identify(InterrogationModel model, GatewayEntities gatewayDb, Entities pnrFoDb, Batch item, BatchRun run)
        {
            var issues = new Models.Issues();

            if (run.CurrentStatus == "Executing task...")
                new IssueBuilder()
                    .SetDescription("This run is still running.")
                    .SetMonitoringLevel(MonitoringLevel.Warning)
                    .BuildAndAdd(issues);

            if (run.CurrentStatus == "Succeeded")
                new IssueBuilder()
                    .SetDescription("This run has succeeded.")
                    .SetMonitoringLevel(MonitoringLevel.Ok)
                    .BuildAndAdd(issues);

            if (run.CurrentStatus == "Failed")
                new IssueBuilder()
                    .SetDescription("This run has failed.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .BuildAndAdd(issues);

            CheckIfBatchEndedPrematurely(issues, run);

            return issues;
        }

        private void CheckIfBatchEndedPrematurely(Models.Issues issues, BatchRun run)
        {
            if (run.CurrentStatus != "Failed" && run.CurrentStatus != "Succeeded") return;
            if(Context.TradeStoreResponse == null || Context.PricingRequests.Value.Count != Context.PricingResponses.Value.Count || Context.RiskDataRequests.Value.Count != Context.RiskDataResponses.Value.Count)
                new IssueBuilder()
                    .SetDescription("It seems like this run completed prematurely.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation("Please check if there are still requests in progress.")
                    .BuildAndAdd(issues);
        }

        public override int GetSequence()
        {
            return 2;
        }
    }
}