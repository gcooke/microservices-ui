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
    public class BatchCheck4BatchHasPricedIssueTracker : BaseBatchIssueTracker
    {
        public override IEnumerable<string> GetDescriptions()
        {
            yield return "Check that batch has pricing calls";
            yield return "Check that pricing requests are complete";
        }

        public override Models.Issues Identify(InterrogationModel model, GatewayEntities gatewayDb, Entities pnrFoDb, Batch item, BatchRun run)
        {
            var issues = new Models.Issues();

            HasPricing(issues);
            if (issues.IssueList.Any()) return issues;
            HasExpectedPricingRequests(issues);
            HasExpectedPricingResponses(issues);

            return issues;
        }

        private void HasPricing(Models.Issues issues)
        {
            var pricingRequests = Context.PricingRequests.Value;

            if (!pricingRequests.Any())
            {
                new IssueBuilder()
                    .SetDescription("This run does not have any Pricing requests.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation("Check the logs for the risk batch request to determine why the pricing request was not made. In most cases you will need to rerun the ENTIRE batch.")
                    .SetShouldContinueCheckingIssues(false)
                    .BuildAndAdd(issues);
            }
        }

        private void HasExpectedPricingRequests(Models.Issues issues)
        {
            var pricingRequests = Context.PricingRequests.Value;
            var expectedPricingRequests = Context.PricingRequests.Value;

            if (pricingRequests.Count != expectedPricingRequests.Count)
            {
                new IssueBuilder()
                    .SetDescription($"This run has made {pricingRequests.Count} pricing requests, but it should have made {expectedPricingRequests} pricing requests.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation("Check the logs for the risk batch request to determine why CERTAIN pricing request was not made. In most cases you will need to rerun only those requests that were not made.")
                    .BuildAndAdd(issues);
            }
            else
            {
                new IssueBuilder()
                    .SetDescription($"This run has made {pricingRequests.Count} pricing requests, which matches the expected pricing request count ({expectedPricingRequests.Count}).")
                    .SetMonitoringLevel(MonitoringLevel.Ok)
                    .BuildAndAdd(issues);
            }
        }

        private void HasExpectedPricingResponses(Models.Issues issues)
        {
            var pricingRequests = Context.PricingRequests.Value;
            var pricingResponses = Context.PricingResponses.Value;
            var successfulResponses = Context.PricingResponses.Value.Count(x => x.Response.ResultCode == 1);

            if (pricingResponses.Count < pricingRequests.Count)
            {
                new IssueBuilder()
                    .SetDescription($"This run only received {pricingResponses.Count} pricing responses. This is less than the pricing requests that was made {pricingRequests.Count}.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation("Check the dashboard for the risk batch request to determine which pricing requests did not get a response. These requests may still be running.")
                    .BuildAndAdd(issues);
            }
            else if (successfulResponses < pricingResponses.Count)
            {
                new IssueBuilder()
                    .SetDescription($"This run has {(pricingResponses.Count - successfulResponses)} FAILED pricing request.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation("Please rerun these pricing requests.")
                    .BuildAndAdd(issues);
            }
            else
            {
                new IssueBuilder()
                    .SetDescription($"This run has received {pricingResponses.Count} pricing responses which is equal to the number of pricing requests that was made ({pricingRequests.Count}).")
                    .SetMonitoringLevel(MonitoringLevel.Ok)
                    .BuildAndAdd(issues);
            }
        }

        public override int GetSequence()
        {
            return 4;
        }
    }
}