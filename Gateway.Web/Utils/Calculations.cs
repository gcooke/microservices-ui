using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gateway.Web.Models.Controller;

namespace Gateway.Web.Utils
{
    public static class Calculations
    {
        public static void SetRelativePercentages(this List<HistoryItem> items)
        {
            var values = items.Where(i => i.TimeTakeMs.HasValue).ToArray();
            var max = values.Length > 0 ? values.Max(i => (long)i.TimeTakeMs.Value) : 0;

            foreach (var item in items)
            {
                if (item.TimeTakeMs.HasValue && max > 0)
                {
                    item.RelativePercentage = (int)((long)item.TimeTakeMs.Value * 100 / max);
                }
            }
        }
    }
}