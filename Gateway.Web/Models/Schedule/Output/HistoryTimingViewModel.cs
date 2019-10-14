using System.Collections.Generic;

namespace Gateway.Web.Models.Schedule.Output
{
    public class HistoryTimingViewModel
    {
        public List<HistoryTimingForScheduleViewModel> HistoryTimingForSchedule { get; set; }
        public string ScheduleName { get; set; }

        public string WallClockTimeTaken { get; set; }
        public string QueueTimeTaken { get; set; }
        public string ProcessTimeTaken { get; set; }

        public string GraphLabels { get; set; }
    }
}