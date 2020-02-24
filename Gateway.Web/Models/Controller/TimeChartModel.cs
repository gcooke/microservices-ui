using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Models.Controller
{
    public class TimeChartModel : BaseControllerModel
    {
        public TimeChartModel(string name) : base(name)
        {
            TimeSummary = new List<ChartItem>();
        }

        public List<ChartItem> TimeSummary { get; private set; }

        public string Key
        {
            get
            {
                {
                    if (TimeSummary.Any())
                        return string.Join(",", TimeSummary.Select(x => $"'{x.Key}'"));
                    else
                        return string.Empty;
                }
            }
        }

        public string Value
        {
            get
            {
                {
                    if (TimeSummary.Any())
                        return string.Join(",", TimeSummary.Select(x => x.Value));
                    else
                        return string.Empty;
                }
            }
        }

        public string Value2
        {
            get
            {
                {
                    if (TimeSummary.Any())
                        return string.Join(",", TimeSummary.Select(x => x.Value2));
                    else
                        return string.Empty;
                }
            }
        }
    }
}