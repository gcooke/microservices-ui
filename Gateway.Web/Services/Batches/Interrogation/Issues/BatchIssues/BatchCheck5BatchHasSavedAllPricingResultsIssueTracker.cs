using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Database;
using Gateway.Web.Services.Batches.Interrogation.Attributes;

namespace Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues
{
    [AppliesToBatch(Models.Enums.Batches.All)]
    public class BatchCheck5BatchHasSavedAllPricingResultsIssueTracker : GenericBatchRequestIssueTracker
    {
        protected override IList<Request> GetRequests()
        {
            return Context.RiskDataRequests.Value;
        }

        protected override IList<RequestResponse> GetResponses()
        {
            return Context.RiskDataResponses.Value;
        }

        protected override IList<RequestResponse> GetExpectedRequests()
        {
            return Context.PricingResponses.Value;
        }

        protected override string GetControllerName()
        {
            return "risk data";
        }

        public override IEnumerable<string> GetDescriptions()
        {
            yield return "Check that pricing and risk data calls match";
            yield return "Check that pricing and risk data calls are complete";
        }

        public override int GetSequence()
        {
            return 5;
        }


        protected override string GetNoRequestRemediation()
        {
            return "One of the following has potentially occurred:<br/>" +
                   "<ul>" +
                   "<li>- The batch is still running and hasn't gotten to risk data yet. <i>If this is the case, just monitor to ensure that risk data requests are made.</i></li>" +
                   "<li>- The batch was killed before any risk data requests was made. <i>If this is the case, determine why the batch was killed, and either rerun the entire batch OR manually save ALL the pricing results.</i></li>" +
                   "<li>- The batch is finished and did not make any risk data requests. <i>If this is the case, either rerun the entire batch OR manually save ALL the pricing results.</i></li>";
        }

        protected override string GetIncorrectRequestsRemediation()
        {
            var requests = Context.RiskDataRequests.Value;
            var expectedRequests = Context.PricingResponses.Value;

            var enumerable = requests.Select(x => GetRemediationString(x.Resource));

            var missing = expectedRequests
                .Select(x => GetPricingString(x.Request.Resource))
                .Where(x => !requests.Select(y => GetRemediationString(y.Resource)).Contains(x))
                .ToList()
                .Take(100)
                .ToList();

            var remediation = "The following risk data requests are missing:<br/>" +
                              $"{string.Join(",", missing.Select(x => $" {x}"))} <i>(showing the first 100 only)</i><br/><br/>" +
                              "Please check the following:<br/>" +
                              "<ul>" +
                              "<li>- If the batch is still running, then these requests probably have yet to be queued.<li>" +
                              "<li>" +
                              "- If the batch has completed (succeeded or failed), then the issue is that these risk data" +
                              "requests were not made. You can trigger these risk data requests manually (recommended) or you " +
                              "can rerun the batch for these missing requests only." +
                              "</li>" +
                              "</ul>" +
                              "<br/>" +
                              "Here is a list that you could potentially copy + paste:<br/>" +
                              $"{GetRemediationString(missing)}";

            return remediation;
        }

        protected override string GetResponsesRemediation()
        {
            var requests = Context.RiskDataRequests.Value;
            var responses = Context.RiskDataResponses.Value;

            var missing = requests
                .Select(x => GetRemediationString(x.Resource))
                .Where(x => !responses.Select(y => GetRemediationString(y.Request.Resource)).Contains(x))
                .ToList()
                .Take(100)
                .ToList();

            var remediation = "The following risk data responses are missing:<br/>" +
                              $"{string.Join(",", missing.Select(x => $" {x}"))} <i>(showing the first 100 only)</i><br/><br/>" +
                              "Please check the following:<br/>" +
                              "<ul>" +
                              "<li>- If the batch is still running, then these requests are probably still in progress.<li>" +
                              "<li>- If the batch has completed (succeeded or failed), then there is an issue where the " +
                              "batch completed before it's children completed. If this is the case you must check each" +
                              "individual request's logs to determine if the requests are still running. If there are" +
                              "let them continue. If they are not then, you can trigger these requests manually (recommended), " +
                              "or you can rerun the batch for these requests only." +
                              "</li>" +
                              "</ul>" +
                              "<br/>" +
                              "Here is a list that you could potentially copy + paste:<br/>" +
                              $"{GetRemediationString(missing)}";

            return remediation;
        }

        protected override string GetUnsuccessfulResponsesRemediation()
        {
            var unsuccessfulResponses = Context
                .RiskDataResponses
                .Value
                .Where(x => x.Response.ResultCode == 0)
                .Select(x => x.Request.Resource)
                .ToList();

            var remediation = "The following risk data responses were unsuccessful:<br/>" +
                              $"{string.Join(",", unsuccessfulResponses.Select(x => $" {x}"))}<br/><br/>" +
                              "Please do ONE of the following:<br/>" +
                              "<ul>" +
                              "<li>- Trigger these requests manually. If they still fail, then revert to the next point." +
                              "</li>" +
                              "<li>- Rerun the batch for these requests only." +
                              "</li>" +
                              "</ul>" +
                              "<br/>" +
                              "Here is a list that you could potentially copy + paste:<br/>" +
                              $"{GetRemediationString(unsuccessfulResponses)}";

            return remediation;
        }

        private string GetRemediationString(IList<string> items)
        {
            try
            {
                var remediationList = items.Select(GetRemediationString).Select(label => $" {label}").ToList();
                return string.Join(",", remediationList);
            }
            catch (Exception ex)
            {
                return string.Join(", ", items);
            }
        }

        private string GetRemediationString(string item)
        {
            var parts = item.Split('?');
            if (parts.Length != 2)
                return item;
            var description = parts[1].Split('=');
            if (description.Length != 2) return parts[1];
            var name = description[1].Split('-');
            if (name.Length != 2) return description[1];
            return name[0].Trim();
        }

        private string GetPricingString(string item)
        {
            var parts = item.Split('-');
            if (parts.Length != 2) return item;
            return parts[0].Trim();
        }
    }
}