using System.Linq;
using Gateway.Web.Database;
using Gateway.Web.Services.Batches.Interrogation.Attributes;
using Gateway.Web.Services.Batches.Interrogation.Models;
using Gateway.Web.Services.Batches.Interrogation.Models.Builders;
using Gateway.Web.Services.Batches.Interrogation.Models.Enums;

namespace Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues
{
    [AppliesToBatch(Models.Enums.Batches.All)]
    public class BatchCheck2BatchStatusIssueTracker : BaseBatchIssueTracker
    {
        public override Models.Issues Identify(GatewayEntities gatewayDb, Batch item)
        {
            var issues = new Models.Issues();

            var latestRun = item.ActualOccurrences.OrderByDescending(x => x.StartedAt).FirstOrDefault();
            if (latestRun == null) return issues;

            if (latestRun.CurrentStatus == "Executing task...")
                new IssueBuilder()
                    .SetDescription("The latest run is still running.")
                    .SetMonitoringLevel(MonitoringLevel.Warning)
                    .BuildAndAdd(issues);

            if (latestRun.CurrentStatus == "Succeeded")
                new IssueBuilder()
                    .SetDescription("The latest run has succeeded.")
                    .SetMonitoringLevel(MonitoringLevel.Ok)
                    .BuildAndAdd(issues);

            if (latestRun.CurrentStatus == "Failed")
                new IssueBuilder()
                    .SetDescription("The latest run has failed.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .BuildAndAdd(issues);

            return issues;
        }

        public override int GetSequence()
        {
            return 2;
        }
    }
}