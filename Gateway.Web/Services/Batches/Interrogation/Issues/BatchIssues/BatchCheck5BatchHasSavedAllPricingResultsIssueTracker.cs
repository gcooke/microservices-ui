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
    public class BatchCheck5BatchHasSavedAllPricingResultsIssueTracker : BaseBatchIssueTracker
    {
        public override IEnumerable<string> GetDescriptions()
        {
            yield return "Check that pricing and risk data calls match";
            yield return "Check that pricing and risk data calls are complete";
        }

        public override Models.Issues Identify(InterrogationModel model, GatewayEntities gatewayDb, Entities pnrFoDb, Batch item, BatchRun run)
        {
            var issues = new Models.Issues();

            var requests = Context.RiskDataRequests.Value;
            var responses = Context.RiskDataResponses.Value;
            var expectedRequests = Context.PricingResponses.Value;

            CheckIfBatchSavedAnyData(issues, requests, responses, expectedRequests);
            if (issues.IssueList.Any()) return issues;
            CheckIfAllSuccessfulPricingRequestsHaveRiskDataRequest(issues, requests, responses, expectedRequests);
            CheckIfAllRiskDataRequestsHaveCompleted(issues, requests, responses, expectedRequests);

            return issues;
        }

        private void CheckIfBatchSavedAnyData(Models.Issues issues, IList<Request> requests, IList<RequestResponse> responses, IList<RequestResponse> expectedRequests)
        {
            if (!requests.Any())
            {
                var remediation = string.Empty;

                if (expectedRequests.Any())
                {
                    var correlationIdList = expectedRequests.Select(x => x.Response.CorrelationId);
                    var pricingRequests = string.Join(",", correlationIdList);
                    remediation = $"The following pricing requests need to be saved:<br/>{pricingRequests}";
                }
                else
                {
                    remediation = "One of the following has potentially occurred:<br/>" +
                                 "<ul>" +
                                 "<li>The batch did not make any pricing requests.</li>" +
                                 "<li>The batch made pricing requests but none of them completed.</li>" +
                                 "</ul>" +
                                 "<br/>" +
                                 "If the first case is true, then the ENTIRE batch needs to be rerun. If the second " +
                                 "case has occurred, then check why they haven't completed. If they are still running " +
                                 "then wait for them to complete and check if the data saves (the logs are a good indication " +
                                 "of whether the calculation is still running or not). If you have determined that the calculations " +
                                 "are not doing anything (very unlikely), then you need to rerun the ENTIRE batch (consider environment reset).";
                }

                new IssueBuilder()
                    .SetDescription("The latest run does not have any Risk Data requests.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation(remediation)
                    .SetShouldContinueCheckingIssues(false)
                    .BuildAndAdd(issues);
            }
        }

        private void CheckIfAllSuccessfulPricingRequestsHaveRiskDataRequest(Models.Issues issues, IList<Request> requests, IList<RequestResponse> responses, IList<RequestResponse> expectedRequests)
        {
            if (requests.Count < expectedRequests.Count)
            {
                var remediation = "Rerun the batch only for those pricing requests that didn't make risk data requests.";
                new IssueBuilder()
                    .SetDescription($"The latest run has made {requests.Count} risk data requests, but it should have made {expectedRequests.Count} risk data requests.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation(remediation)
                    .BuildAndAdd(issues);
            }
            else if (requests.Count > expectedRequests.Count)
            {
                new IssueBuilder()
                    .SetDescription($"The latest run has made {requests.Count} risk data requests, which is more than the expected risk data request count ({expectedRequests.Count}).")
                    .SetMonitoringLevel(MonitoringLevel.Warning)
                    .SetRemediation("This isn't necessarily an issue, but this usually occurs when the database writes are slow. So you may want " +
                                    "monitor the database performance.")
                    .BuildAndAdd(issues);
            }
            else
            {
                new IssueBuilder()
                    .SetDescription($"The latest run has made {requests.Count} risk data requests, which matches the expected risk data request count ({expectedRequests.Count}).")
                    .SetMonitoringLevel(MonitoringLevel.Ok)
                    .BuildAndAdd(issues);
            }
        }

        private void CheckIfAllRiskDataRequestsHaveCompleted(Models.Issues issues, IList<Request> requests, IList<RequestResponse> responses, IList<RequestResponse> expectedRequests)
        {
            if (responses.Count < requests.Count)
            {
                var requestsWithoutResponse = requests.Select(x => x.CorrelationId)
                    .Where(x => !responses.Select(y => y.Response.CorrelationId).Contains(x))
                    .ToList();
                var list = string.Join(",", requestsWithoutResponse);

                new IssueBuilder()
                    .SetDescription($"The latest run only received {responses.Count} risk data responses. This is less than the risk data requests that was made. The requests that didn't receive responses are {list}.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation("Rerun the batch only for those risk data requests that get responses.")
                    .BuildAndAdd(issues);
            }
            else if(responses.Count == requests.Count)
            {
                new IssueBuilder()
                    .SetDescription($"The latest run has received {responses.Count} risk data responses which equal to the number of risk data requests that was made ({requests.Count}).")
                    .SetMonitoringLevel(MonitoringLevel.Ok)
                    .BuildAndAdd(issues);
            }
        }

        public override int GetSequence()
        {
            return 5;
        }
    }
}