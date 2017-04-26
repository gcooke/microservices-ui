using System.Collections.Generic;

namespace Gateway.Web.Models.Controller
{
    public class RequestsChartModel : BaseControllerModel
    {
        public RequestsChartModel(string name) : base(name)
        {
            RequestSummary = new List<ChartItem>();
        }

        public List<ChartItem> RequestSummary { get; private set; }
    }

    public class ChartItem
    {
        public ChartItem(string key, int value)
        {
            Key = key;
            Value = value;
        }

        public ChartItem(string key, int value, int value2)
        {
            Key = key;
            Value = value;
            Value2 = value2;
        }

        public string Key { get; set; }
        public int Value { get; set; }
        public int Value2 { get; set; }
    }
}