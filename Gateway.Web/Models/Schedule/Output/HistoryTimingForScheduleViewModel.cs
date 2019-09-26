using Gateway.Web.Utils;
using System;

namespace Gateway.Web.Models.Schedule.Output
{
    public class HistoryTimingForScheduleViewModel
    {
        public DateTime RunDate { get; set; }
        public long WallClockTime { get; set; }
        public long TotalQueueTime { get; set; }
        public long TotalProcessTime { get; set; }
        public long ScheduleId { get; set; }

        public string WallClockTimeHumanize => new TimeSpan(0, 0, 0, 0, (int)WallClockTime).Humanize();
        public string TotalQueueTimeHumanize => new TimeSpan(0, 0, 0, 0, (int)TotalQueueTime).Humanize();
        public string TotalProcessTimeHumanize => new TimeSpan(0, 0, 0, 0, (int)TotalProcessTime).Humanize();
    }
}