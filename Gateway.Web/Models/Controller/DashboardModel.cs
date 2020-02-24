using System;

namespace Gateway.Web.Models.Controller
{
    public class DashboardModel : BaseControllerModel
    {
        public DashboardModel(string name) : base(name)
        {
            HistoryStartTime = DateTime.Today.AddDays(-7);
        }

        public DateTime HistoryStartTime { get; set; }
        public int TotalCalls { get; set; }
        public int TotalErrors { get; set; }
        public string AverageResponse { get; set; }
        public TimeChartModel TimeChart { get; set; }
        public RequestsChartModel RequestsChart { get; set; }
    }
}