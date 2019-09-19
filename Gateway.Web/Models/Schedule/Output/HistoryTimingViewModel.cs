using System.Collections.Generic; 

namespace Gateway.Web.Models.Schedule.Output
{
    public class HistoryTimingViewModel
    {
        public List<HistoryTimingForScheduleViewModel> HistoryTimingForSchedule { get; set; }
        public string ScheduleName { get; set; }
    }
}