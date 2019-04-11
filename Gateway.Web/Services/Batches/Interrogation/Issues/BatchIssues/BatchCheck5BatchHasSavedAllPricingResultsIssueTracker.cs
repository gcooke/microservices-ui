using System.Linq;
using Gateway.Web.Database;
using Gateway.Web.Services.Batches.Interrogation.Attributes;
using Gateway.Web.Services.Batches.Interrogation.Models;
using Gateway.Web.Services.Batches.Interrogation.Models.Builders;
using Gateway.Web.Services.Batches.Interrogation.Models.Enums;

namespace Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues
{
    [AppliesToBatch(Models.Enums.Batches.All)]
    public class BatchCheck5BatchHasSavedAllPricingResultsIssueTracker : BaseBatchIssueTracker
    {
        public override Models.Issues Identify(GatewayEntities gatewayDb, Batch item)
        {
            var issues = new Models.Issues();

            var latestRun = item.ActualOccurrences.OrderByDescending(x => x.StartedAt).FirstOrDefault();
            if (latestRun == null) return issues;

            var correlationId = latestRun.CorrelationId;

            var riskDataRequests = gatewayDb
                .Requests
                .Where(x => x.ParentCorrelationId == correlationId)
                .Where(x => x.Controller.ToLower() == "riskdata")
                .ToList();

            if (!riskDataRequests.Any())
            {
                new IssueBuilder()
                    .SetDescription("The latest run does not have any Pricing requests.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation("Rerun the ENTIRE batch.")
                    .SetShouldContinueCheckingIssues(false)
                    .BuildAndAdd(issues);
                return issues;
            }

            var expectedRiskDataRequests = DetermineExpectedRiskDataRequestCount(gatewayDb, item);

            if (riskDataRequests.Count < expectedRiskDataRequests)
            {
                new IssueBuilder()
                    .SetDescription($"The latest run has made {riskDataRequests.Count} risk data requests, but it should have made {expectedRiskDataRequests} risk data requests.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation("Rerun the batch only for those pricing requests that didn't make risk data requests.")
                    .BuildAndAdd(issues);
            }
            else if (riskDataRequests.Count > expectedRiskDataRequests)
            {
                new IssueBuilder()
                    .SetDescription($"The latest run has made {riskDataRequests.Count} risk data requests, which is more than the expected risk data request count ({expectedRiskDataRequests}).")
                    .SetMonitoringLevel(MonitoringLevel.Warning)
                    .BuildAndAdd(issues);
            }
            else
            {
                new IssueBuilder()
                    .SetDescription($"The latest run has made {riskDataRequests.Count} risk data requests, which matches the expected risk data request count ({expectedRiskDataRequests}).")
                    .SetMonitoringLevel(MonitoringLevel.Ok)
                    .BuildAndAdd(issues);
            }

            var requestCorrelationIds = riskDataRequests.Select(y => y.CorrelationId).ToList();
            var riskDataResponses = gatewayDb
                .Responses
                .Where(x => requestCorrelationIds.Contains(x.CorrelationId))
                .ToList();

            if (riskDataResponses.Count < riskDataRequests.Count)
            {
                new IssueBuilder()
                    .SetDescription($"The latest run only received {riskDataResponses.Count} risk data responses. This is less than the risk data requests that was made. Please investigate.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation("Rerun the batch only for those risk data requests that get responses.")
                    .BuildAndAdd(issues);
            }
            else if (riskDataResponses.Count > riskDataRequests.Count)
            {
                new IssueBuilder()
                    .SetDescription($"The latest run has received {riskDataResponses.Count} risk data responses which is more than the number of risk data requests that was made ({riskDataRequests.Count}).")
                    .SetMonitoringLevel(MonitoringLevel.Warning)
                    .BuildAndAdd(issues);
            }
            else 
            {
                new IssueBuilder()
                    .SetDescription($"The latest run has received {riskDataResponses.Count} risk data responses which equal to the number of risk data requests that was made ({riskDataRequests.Count}).")
                    .SetMonitoringLevel(MonitoringLevel.Ok)
                    .BuildAndAdd(issues);
            }

            return issues;
        }

        private int DetermineExpectedRiskDataRequestCount(GatewayEntities gatewayDb, Batch item)
        {
            var latestRun = item.ActualOccurrences.OrderByDescending(x => x.StartedAt).First();
            var correlationId = latestRun.CorrelationId;

            var pricingRequests = gatewayDb
                .Requests
                .Where(x => x.ParentCorrelationId == correlationId)
                .Where(x => x.Controller.ToLower() == "pricing")
                .ToList();

            var requestCorrelationIds = pricingRequests.Select(y => y.CorrelationId).ToList();
            var pricingResponses = gatewayDb
                .Responses
                .Where(x => requestCorrelationIds.Contains(x.CorrelationId))
                .ToList();

            return pricingResponses.Count;
        }

        public override int GetSequence()
        {
            return 5;
        }
    }
}