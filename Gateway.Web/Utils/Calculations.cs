using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Models.Controller;

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

                case "result":
                    target.AddRange(items.OrderBy(i => i.ResultFormatted));
                    break;
                case "result_desc":
                    target.AddRange(items.OrderByDescending(i => i.ResultFormatted));
                    break;
            }
        }

        public static void SetRelativePercentages(this List<HistoryItem> items)
        {
            var values = items.Where(i => i.TimeTakeMs.HasValue).ToArray();
            var max = Math.Abs(values.Length > 0 ? values.Max(i => (long)(i.ActualTimeTakenMs ?? 0)) : 0);

            foreach (var item in items)
            {
                if (item.ActualTimeTakenMs.HasValue && max > 0)
                {
                    item.RelativePercentage = Math.Abs((int)((long)item.ActualTimeTakenMs.Value * 100 / max));
                }
            }
        }
    }
}