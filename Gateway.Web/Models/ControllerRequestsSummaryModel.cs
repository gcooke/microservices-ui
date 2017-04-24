using System;
using System.Collections.Generic;

namespace Gateway.Web.Models
{
    public class ControllerRequestsSummaryModel
    {
        public ControllerRequestsSummaryModel()
        {
            RequestSummary = new List<ChartItem>();
        }

        public string Name { get; set; }
        public List<ChartItem> RequestSummary { get; private set; }
    }

    public class ChartItem
    {
        public ChartItem(string key, int value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public int Value { get; set; }
    }
}