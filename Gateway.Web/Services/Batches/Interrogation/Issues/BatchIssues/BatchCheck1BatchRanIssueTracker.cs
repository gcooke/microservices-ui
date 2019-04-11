using System;
using System.Linq;
using Gateway.Web.Database;
using Gateway.Web.Services.Batches.Interrogation.Attributes;
using Gateway.Web.Services.Batches.Interrogation.Models;
using Gateway.Web.Services.Batches.Interrogation.Models.Builders;
using Gateway.Web.Services.Batches.Interrogation.Models.Enums;

namespace Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues
{
    [AppliesToBatch(Models.Enums.Batches.All)]
    public class BatchCheck1BatchRanIssueTracker : BaseBatchIssueTracker
    {
        public override Models.Issues Identify(GatewayEntities gatewayDb, Entities pnrFoDb, Batch item, BatchRun run)
        {
            var issues = new Models.Issues();

            var now = DateTime.Now;
            var occurrencesAsOfNow = item.ExpectedOccurrences.Where(x => x <= now).ToList();

            if (occurrencesAsOfNow.Any() && item.ActualOccurrences.Count == 0)
                new IssueBuilder()
                    .SetDescription($"This batch should have run {occurrencesAsOfNow.Count} time(s) as of {now:yyyy-MM-dd hh:mm} but HAS NOT RUN.")
                    .SetMonitoringLevel(MonitoringLevel.Error)
                    .SetRemediation("Check the scheduler - it may not be running. If it is, rerun the batch from the dashboard")
                    .SetShouldContinueCheckingIssues(false)
                    .BuildAndAdd(issues);

            if (!occurrencesAsOfNow.Any())
                new IssueBuilder()
                    .SetDescription("This batch is not scheduled to run for today.")
                    .SetMonitoringLevel(MonitoringLevel.Info)
                    .SetShouldContinueCheckingIssues(false)
                    .BuildAndAdd(issues);

            if (occurrencesAsOfNow.Any() && item.ActualOccurrences.Count > 0)
            {
                new IssueBuilder()
                    .SetDescription($"This batch should have run {occurrencesAsOfNow.Count} time(s) as of {now:yyyy-MM-dd hh:mm} and HAS RUN {item.ActualOccurrences.Count} times(s). ")
                    .SetMonitoringLevel(MonitoringLevel.Info)
                    .BuildAndAdd(issues);
            }

            return issues;
        }
        public override int GetSequence()
        {
            return 1;
        }
    }
}