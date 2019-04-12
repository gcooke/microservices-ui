using System;
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
    public class BatchCheck4BatchHasPricedIssueTracker : GenericBatchRequestIssueTracker
    {

        protected override IList<Request> GetRequests()
        {
            return Context.PricingRequests.Value;
        }

        protected override IList<RequestResponse> GetResponses()
        {
            return Context.PricingResponses.Value;
        }

        protected override IList<RequestResponse> GetExpectedRequests()
        {
            return Context.PricingResponses.Value;
        }

        protected override string GetControllerName()
        {
            return "pricing";
        }

        public override IEnumerable<string> GetDescriptions()
        {
            yield return "Check that batch has pricing calls";
            yield return "Check that pricing requests are complete";
        }

        public override int GetSequence()
        {
            return 4;
        }

        protected override string GetNoRequestRemediation()
        {
            return "One of the following has potentially occurred:<br/>" +
                   "<ul>" +
                   "<li>- The batch is still running and hasn't gotten to pricing yet. <i>If this is the case, just monitor to ensure that pricing requests are made.</i></li>" +
                   "<li>- The batch was killed before any pricing requests was made. <i>If this is the case, determine why the batch was killed, and potentially rerun the entire batch.</i></li>" +
                   "<li>- The batch is finished and did not make any pricing requests. <i>If the last case is true, then the ENTIRE batch needs to be rerun.</i></li>";
        }

        protected override string GetIncorrectRequestsRemediation()
        {
            var requests = Context.PricingRequests.Value;
            var expectedRequests = Context.PricingRequests.Value;

            var missing = expectedRequests
                .Select(x => x.Resource)
                .Where(x => requests.Select(y => y.Resource).Contains(x))
                .ToList()
                .Take(100)
                .ToList();

            var remediation = "The following pricing requests are missing:<br/>" +
                              $"{string.Join(",", missing)} <i>(showing the first 100 only)</i><br/><br/>" +
                              "Please check the following:<br/>" +
                              "<ul>" +
                              "<li>- If the batch is still running, then these requests probably have yet to be queued.<li>" +
                              "<li>" +
                              "- If the batch has completed (succeeded or failed), then the issue is that these pricing" +
                              "requests were not made. Please rerun the batch for these requests only." +
                              "</li>" +
                              "</ul>" +
                              "<br/>" +
                              "Here is a list that you could potentially copy + paste:<br/>" +
                              $"{GetRemediationString(missing)}";

            return remediation;
        }

        protected override string GetResponsesRemediation()
        {
            var requests = Context.PricingRequests.Value;
            var responses = Context.PricingResponses.Value;

            var missing = requests
                .Select(x => x.Resource)
                .Where(x => responses.Select(y => y.Request.Resource).Contains(x))
                .ToList()
                .Take(100)
                .ToList();

            var remediation = "The following pricing responses are missing:<br/>" +
                              $"{string.Join(",", missing)} <i>(showing the first 100 only)</i><br/><br/>" +
                              "Please check the following:<br/>" +
                              "<ul>" +
                              "<li>- If the batch is still running, then these requests are probably still in progress.<li>" +
                              "<li>- If the batch has completed (succeeded or failed), then there is an issue where the " +
                              "batch completed before it's children completed. If this is the case you must check each" +
                              "individual request's logs to determine if the requests are still running. If there are" +
                              "let them continue. If they are not then you will need to rerun the batch for these " +
                              "requests only." +
                              "</li>" +
                              "</ul>" +
                              "<br/>" +
                              "Here is a list that you could potentially copy + paste:<br/>" +
                              $"{GetRemediationString(missing)}";

            return remediation;
        }

        protected override string GetUnsuccessfulResponsesRemediation()
        {
            var responses = Context.PricingRequests.Value;
            var unsuccessfulResponses = Context.PricingResponses.Value.Where(x => x.Response.ResultCode == 0);

            var missing = responses
                .Select(x => x.Resource)
                .Where(x => unsuccessfulResponses.Select(y => y.Request.Resource).Contains(x))
                .ToList()
                .Take(100)
                .ToList();

            var remediation = "The following pricing responses were unsuccessful:<br/>" +
                              $"{string.Join(",", missing)} <i>(showing the first 100 only)</i><br/><br/>" +
                              "Please do the following:<br/>" +
                              "<ul>" +
                              "<li>- Rerun the batch for these pricing requests only." +
                              "</li>" +
                              "</ul>" +
                              "<br/>" +
                              "Here is a list that you could potentially copy + paste:<br/>" +
                              $"{GetRemediationString(missing)}";

            return remediation;
        }

        private string GetRemediationString(IList<string> items)
        {
            try
            {
                var remediationList = items.Select(item => item.Split('-')).Select(label => $" {label[0].Trim()}").ToList();
                return string.Join(",", remediationList);
            }
            catch (Exception ex)
            {
                return string.Join(",", items);
            }
        }
    }
}