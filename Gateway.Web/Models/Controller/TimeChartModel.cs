using System.Collections.Generic;

namespace Gateway.Web.Models.Controller
{
    public class TimeChartModel : BaseControllerModel
    {
        public TimeChartModel(string name) : base(name)
        {
            TimeSummary = new List<ChartItem>();
        }

        public List<ChartItem> TimeSummary { get; private set; }
    }    
}