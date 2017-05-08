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
            var total = items.Where(i => i.TimeTakeMs.HasValue).Average(i => (long)i.TimeTakeMs.Value);

            foreach (var item in items)
            {
                if (item.TimeTakeMs.HasValue)
                {
                    if (item.TimeTakeMs.Value > total)
                        item.RelativePercentage = 100;
                    else
                        item.RelativePercentage = (int)(item.TimeTakeMs.Value * 100 / total);
                }
            }
        }
    }
}