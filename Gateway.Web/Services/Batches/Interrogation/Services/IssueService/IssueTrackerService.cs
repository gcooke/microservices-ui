using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gateway.Web.Services.Batches.Interrogation.Attributes;
using Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues;

namespace Gateway.Web.Services.Batches.Interrogation.Services.IssueService
{
    public class IssueTrackerService : IIssueTrackerService
    {
        public IEnumerable<BaseBatchIssueTracker> GetIssueTrackersForBatch(string batchName)
        {
            var type = typeof(BaseBatchIssueTracker);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p))
                .Where(p => p.GetCustomAttribute(typeof(AppliesToBatchAttribute)) != null)
                .ToList();

            var issues = new List<BaseBatchIssueTracker>();
            var name = batchName.Replace(".", "");

            foreach (var item in types)
            {
                var attribute = item.GetCustomAttribute<AppliesToBatchAttribute>();

                if (string.Equals(attribute.Batch.ToString(), name, StringComparison.CurrentCultureIgnoreCase) &&
                    string.Equals(attribute.Batch.ToString(), "All", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                var issue = (BaseBatchIssueTracker)Activator.CreateInstance(item);
                issues.Add(issue);
            }

            return issues.OrderBy(x => x.GetSequence());
        }
    }
}