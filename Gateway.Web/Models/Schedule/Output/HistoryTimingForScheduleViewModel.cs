using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Models.Schedule.Output
{
    public class HistoryTimingForScheduleViewModel
    {
        public DateTime RunDate { get; set; }
        public long WallClockTime { get; set; }
        public long TotalQueueTime { get; set; }
        public long TotalProcessTime { get; set; }
        public long ScheduleId { get; set; }
    }
}