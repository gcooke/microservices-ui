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
    public class BatchCheck3BatchHasTradesIssueTracker : BaseBatchIssueTracker
    {
        public override IEnumerable<string> GetDescriptions()
        {
            yield return "Check that trades are loaded";
        }

        public override Models.Issues Identify(InterrogationModel model, GatewayEntities gatewayDb, Entities pnrFoDb, Batch item)
        {
            var issues = new Models.Issues();

            HasTradeStoreRequest(issues);
            if (issues.IssueList.Any()) return issues;
            HasTradeStoreResponse(issues);
            if (issues.IssueList.Any()) return issues;
            HasTrades(issues);
            if (issues.IssueList.Any()) return issues;

            var tradeStoreResponse = Context.TradeStoreResponse.Value;
            new IssueBuilder()
                .SetDescription($"The latest run has made a TradeStore request, received a response with {tradeStoreResponse.Response.Size} trade(s).")
                .SetMonitoringLevel(MonitoringLevel.Ok)
                .BuildAndAdd(issues);

            return issues;
        }

        public void HasTradeStoreRequest(Models.Issues issues)
        {
            var tradeStoreRequest = Context.TradeStoreRequest.Value;

            if (tradeStoreRequest == null)
            {
                new IssueBuilder()
                    .SetDescription("The latest run does not have a TradeStore request.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation("Rerun the ENTIRE batch.")
                    .SetShouldContinueCheckingIssues(false)
                    .BuildAndAdd(issues);
            }
        }

        public void HasTradeStoreResponse(Models.Issues issues)
        {
            var tradeStoreResponse = Context.TradeStoreResponse.Value;
            if (tradeStoreResponse == null)
            {
                new IssueBuilder()
                    .SetDescription("The latest run has made a TradeStore request but did not receive a response.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation("Rerun the ENTIRE batch.")
                    .SetShouldContinueCheckingIssues(false)
                    .BuildAndAdd(issues);
            }
        }

        public void HasTrades(Models.Issues issues)
        {
            var tradeStoreResponse = Context.TradeStoreResponse.Value;
            if (tradeStoreResponse.Response.Size == 0)
            {
                new IssueBuilder()
                    .SetDescription("The latest run has made a TradeStore request, received a response but the response has no trades.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation("Rerun the ENTIRE batch.")
                    .SetShouldContinueCheckingIssues(false)
                    .BuildAndAdd(issues);
            }
        }

        public override int GetSequence()
        {
            return 3;
        }
    }
}