using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gateway.Web.Database;
using Gateway.Web.Models.Interrogation;
using Gateway.Web.Services.Batches.Interrogation.Models;
using Gateway.Web.Services.Batches.Interrogation.Models.Builders;
using Gateway.Web.Services.Batches.Interrogation.Models.Enums;

namespace Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues
{
    public abstract class GenericBatchRequestIssueTracker : BaseBatchIssueTracker
    {
        public override Models.Issues Identify(InterrogationModel model, GatewayEntities gatewayDb, Entities pnrFoDb, Batch item, BatchRun run)
        {
            var issues = new Models.Issues();

            CheckRequests(issues);
            if (issues.IssueList.Any(x => !x.ShouldContinueCheckingIssues)) return issues;

            CheckCorrectRequests(issues);
            if (issues.IssueList.Any(x => !x.ShouldContinueCheckingIssues)) return issues;

            CheckResponses(issues);
            if (issues.IssueList.Any(x => !x.ShouldContinueCheckingIssues)) return issues;

            CheckSuccessfulResponses(issues);
            if (issues.IssueList.Any()) return issues;

            CheckNoIssues(issues);

            return issues;
        }

        #region Checks
        private void CheckNoIssues(Models.Issues issues)
        {
            var requests = GetRequests();
            var responses = GetResponses();
            var expectedResponses = GetExpectedRequests();
            var successfulResponses = GetExpectedRequests().Where(x => x.Response.ResultCode == 1).ToList();

            var description = $"There was {requests.Count} {GetControllerName()} requests.<br/>" +
                              $"There was {responses.Count} {GetControllerName()} responses<br/>" +
                              $"The expected number of {GetControllerName()} requests was {expectedResponses.Count}.<br/>" +
                              $"The was {successfulResponses.Count} successful {GetControllerName()} responses.";
            var issue = GetIssue(description, null, MonitoringLevel.Ok);
            issues.Add(issue);
        }

        private void CheckRequests(Models.Issues issues)
        {
            var requests = GetRequests();
            if (requests.Any()) return;
            var description = $"No {GetControllerName()} requests were made.";
            var remediation = GetNoRequestRemediation();
            var issue = GetIssue(description, remediation, MonitoringLevel.Error, false);
            issues.Add(issue);
        }

        protected abstract string GetNoRequestRemediation();

        private void CheckCorrectRequests(Models.Issues issues)
        {
            var requests = GetRequests();
            var expectedRequests = GetExpectedRequests();

            if (requests.Count == expectedRequests.Count) return;
            if (requests.Count > expectedRequests.Count)
            {
                var d = $"There should only have been {expectedRequests.Count} {GetControllerName()} requests, but there was {requests.Count} requests which is more than expected.";
                var i = GetIssue(d, "This isn't necessarily an issue, but please investigate.", MonitoringLevel.Warning);
                issues.Add(i);
                return;
            }

            var description = $"There should have been {expectedRequests.Count} {GetControllerName()} requests, but there was only {requests.Count} requests.";
            var remediation = GetIncorrectRequestsRemediation();
            var issue = GetIssue(description, remediation, MonitoringLevel.Error);
            issues.Add(issue);
        }

        protected abstract string GetIncorrectRequestsRemediation();

        private void CheckResponses(Models.Issues issues)
        {
            var requests = GetRequests();
            var responses = GetResponses();

            if (requests.Count == responses.Count) return;
            if (responses.Count > requests.Count)
            {
                var d = $"There should only have been {requests.Count} {GetControllerName()} responses, but there was {responses.Count} requests which is more than expected.";
                var i = GetIssue(d, "This isn't necessarily an issue, but please investigate.", MonitoringLevel.Warning);
                issues.Add(i);
                return;
            };

            var description = $"There should have been {requests.Count} {GetControllerName()} responses, but there was only {responses.Count} requests.";
            var remediation = GetResponsesRemediation();
            var issue = GetIssue(description, remediation, MonitoringLevel.Error);
            issues.Add(issue);
        }

        protected abstract string GetResponsesRemediation();

        private void CheckSuccessfulResponses(Models.Issues issues)
        {
            var responses = GetResponses();
            var successfulResponses = GetResponses().Where(x => x.Response.ResultCode == 1).ToList();

            if (responses.Count == successfulResponses.Count) return;

            var description = $"There should have been {responses.Count} SUCCESSFUL {GetControllerName()} responses, but there was only {successfulResponses.Count} responses.";
            var remediation = GetUnsuccessfulResponsesRemediation();
            var issue = GetIssue(description, remediation, MonitoringLevel.Error);
            issues.Add(issue);
        }

        protected abstract string GetUnsuccessfulResponsesRemediation();

        #endregion

        protected abstract IList<Request> GetRequests();
        protected abstract IList<RequestResponse> GetResponses();
        protected abstract IList<RequestResponse> GetExpectedRequests();
        protected abstract string GetControllerName();

        public abstract override IEnumerable<string> GetDescriptions();
        public abstract override int GetSequence();
        

        private Issue GetIssue(string description, string remediation, MonitoringLevel monitoringLevel, bool continueChecking = true)
        {
            return new IssueBuilder()
                .SetDescription(description)
                .SetMonitoringLevel(monitoringLevel)
                .SetRemediation(remediation)
                .SetShouldContinueCheckingIssues(continueChecking)
                .Build();
        }
    }
}