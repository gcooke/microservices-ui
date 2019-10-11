using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Models.Controller;
using Gateway.Web.Services;

namespace Gateway.Web.Utils
{
    public static class Calculations
    {
        public static void AddRange(this List<HistoryItem> target, IEnumerable<HistoryItem> items, string sortColumn)
        {
            switch (sortColumn.ToLower())
            {
                case "controller":
                    target.AddRange(items.OrderBy(i => i.Controller));
                    break;
                case "controller_desc":
                    target.AddRange(items.OrderByDescending(i => i.Controller));
                    break;

                case "version":
                    target.AddRange(items.OrderBy(i => i.Version));
                    break;
                case "version_desc":
                    target.AddRange(items.OrderByDescending(i => i.Version));
                    break;

                case "user":
                    target.AddRange(items.OrderBy(i => i.User));
                    break;
                case "user_desc":
                    target.AddRange(items.OrderByDescending(i => i.User));
                    break;

                case "resource":
                    target.AddRange(items.OrderBy(i => i.Resource));
                    break;
                case "resource_desc":
                    target.AddRange(items.OrderByDescending(i => i.Resource));
                    break;

                case "time":
                    target.AddRange(items.OrderBy(i => i.StartUtc));
                    break;
                case "time_desc":
                    target.AddRange(items.OrderByDescending(i => i.StartUtc));
                    break;

                case "timetaken":
                    target.AddRange(items.OrderBy(i => i.TimeTakeMs));
                    break;
                case "timetaken_desc":
                    target.AddRange(items.OrderByDescending(i => i.TimeTakeMs));
                    break;

                case "queuetime":
                    target.AddRange(items.OrderBy(i => i.QueueTimeMs));
                    break;
                case "queuetime_desc":
                    target.AddRange(items.OrderByDescending(i => i.QueueTimeMs));
                    break;

                case "result":
                    target.AddRange(items.OrderBy(i => i.ResultFormatted));
                    break;
                case "result_desc":
                    target.AddRange(items.OrderByDescending(i => i.ResultFormatted));
                    break;
            }
        }

        public static void EnrichHistoryResults(this List<HistoryItem> items, IBatchNameService batchNameService, IUsernameService usernameService)
        {
            SetRelativePercentages(items);
            ReplaceResourceNames(items, batchNameService);
            ReplaceUserNames(items, usernameService);
        }

        private static void SetRelativePercentages(List<HistoryItem> items)
        {
            var values = items.Where(i => i.TimeTakeMs.HasValue).ToArray();
            var maxp = Math.Abs(values.Length > 0 ? values.Max(i => (long)(i.ActualTimeTakenMs ?? 0)) : 0);
            var maxq = Math.Abs(values.Length > 0 ? values.Max(i => (long)(i.QueueTimeMs ?? 0)) : 0);

            foreach (var item in items)
            {
                if (item.ActualTimeTakenMs.HasValue && maxp > 0)
                {
                    item.RelativePercentageP = Math.Abs((int)((long)item.ActualTimeTakenMs.Value * 100 / maxp));
                }

                if (item.QueueTimeMs.HasValue && maxq > 0)
                {
                    item.RelativePercentageQ = Math.Abs((int)((long)item.QueueTimeMs.Value * 100 / maxq));
                }
            }
        }

        private static void ReplaceResourceNames(List<HistoryItem> items, IBatchNameService batchNameService)
        {
            foreach (var item in items)
            {
                if (string.Equals(item.Controller, "RiskBatch", StringComparison.CurrentCultureIgnoreCase))
                    if (item.Resource.StartsWith("Batch/Run/"))
                        item.Resource = batchNameService.GetName(item.Resource);
            }
        }

        private static void ReplaceUserNames(List<HistoryItem> items, IUsernameService usernameService)
        {
            foreach (var item in items)
            {
                item.UserDisplayName = usernameService.GetFullNameFast(item.User);
            }
        }
    }
}