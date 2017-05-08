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
            var max = items.Where(i => i.TimeTakeMs.HasValue).Max(i => (long)i.TimeTakeMs.Value);

            foreach (var item in items)
            {
                if (item.TimeTakeMs.HasValue)
                {
                    item.RelativePercentage = (int)((long)item.TimeTakeMs.Value * 100 / max);
                }
            }
        }
    }
}