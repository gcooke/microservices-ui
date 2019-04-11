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
        public override Models.Issues Identify(InterrogationModel model, GatewayEntities gatewayDb, Entities pnrFoDb, Batch item)
        {
            var issues = new Models.Issues();

            var latestRun = item.ActualOccurrences.OrderByDescending(x => x.StartedAt).FirstOrDefault();
            if (latestRun == null) return issues;

            var correlationId = latestRun.CorrelationId;

            var pricingRequests = gatewayDb
                .Requests
                .Where(x => x.ParentCorrelationId == correlationId)
                .Where(x => x.Controller.ToLower() == "pricing")
                .ToList();

            if (!pricingRequests.Any())
            {
                new IssueBuilder()
                    .SetDescription("The latest run does not have any Pricing requests.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation("Check the logs for the risk batch request to determine why the pricing request was not made. In most cases you will need to rerun the ENTIRE batch.")
                    .SetShouldContinueCheckingIssues(false)
                    .BuildAndAdd(issues);
                return issues;
            }

            var expectedPricingRequests = DetermineExpectedPricingRequestCount(gatewayDb, item);

            if (pricingRequests.Count != expectedPricingRequests)
            {
                new IssueBuilder()
                    .SetDescription($"The latest run has made {pricingRequests.Count} pricing requests, but it should have made {expectedPricingRequests} pricing requests.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation("Check the logs for the risk batch request to determine why CERTAIN pricing request was not made. In most cases you will need to rerun only those requests that were not made.")
                    .BuildAndAdd(issues);
            }
            else
            {
                new IssueBuilder()
                    .SetDescription($"The latest run has made {pricingRequests.Count} pricing requests, which matches the expected pricing request count ({expectedPricingRequests}).")
                    .SetMonitoringLevel(MonitoringLevel.Ok)
                    .BuildAndAdd(issues);
            }

            var requestCorrelationIds = pricingRequests.Select(y => y.CorrelationId).ToList();
            var pricingResponses = gatewayDb
                .Responses
                .Where(x => requestCorrelationIds.Contains(x.CorrelationId))
                .ToList();

            if (pricingResponses.Count < pricingRequests.Count)
            {
                new IssueBuilder()
                    .SetDescription($"The latest run only received {pricingResponses.Count} pricing responses. This is less than the pricing requests that was made. Please investigate.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation("Check the dashboard for the risk batch request to determine which pricing requests did not get a response. Rerun only those requests that did not get a response.")
                    .BuildAndAdd(issues);
            }
            else
            {
                new IssueBuilder()
                    .SetDescription($"The latest run has received {pricingResponses.Count} pricing responses which is equal to the number of pricing requests that was made ({pricingRequests.Count}).")
                    .SetMonitoringLevel(MonitoringLevel.Ok)
                    .BuildAndAdd(issues);
            }

            return issues;
        }

        public override int GetSequence()
        {
            return 4;
        }

        private int DetermineExpectedPricingRequestCount(GatewayEntities gatewayDb, Batch item)
        {
            var latestRun = item.ActualOccurrences.OrderByDescending(x => x.StartedAt).First();
            var correlationId = latestRun.CorrelationId;
            var pricingRequests = gatewayDb
                .Requests
                .Where(x => x.ParentCorrelationId == correlationId)
                .Where(x => x.Controller.ToLower() == "pricing")
                .ToList();
            return pricingRequests.Count;
        }
    }
}