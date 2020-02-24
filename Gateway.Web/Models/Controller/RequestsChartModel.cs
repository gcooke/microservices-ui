using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Models.Controller
{
    public class RequestsChartModel : BaseControllerModel
    {
        public RequestsChartModel(string name) : base(name)
        {
            RequestSummary = new List<ChartItem>();
        }

        public List<ChartItem> RequestSummary { get; private set; }

        public string Key
        {
            get
            {
                {
                    if (RequestSummary.Any())
                        return string.Join(",", RequestSummary.Select(x => $"'{x.Key}'"));
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
                    if (RequestSummary.Any())
                        return string.Join(",", RequestSummary.Select(x => x.Value));
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
                    if (RequestSummary.Any())
                        return string.Join(",", RequestSummary.Select(x => x.Value2));
                    else
                        return string.Empty;
                }
            }
        }
    }

    public class ChartItem
    {
        public ChartItem(string key, long value)
        {
            Key = key;
            Value = value;
        }

        public ChartItem(string key, long value, long value2)
        {
            Key = key;
            Value = value;
            Value2 = value2;
        }

        public string Key { get; set; }
        public long Value { get; set; }
        public long Value2 { get; set; }
    }
}