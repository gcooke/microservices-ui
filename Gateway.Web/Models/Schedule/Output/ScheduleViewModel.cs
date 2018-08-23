using System;

namespace Gateway.Web.Models.Schedule.Output
{
    public class ScheduleViewModel
    {
        public DateTime BusinessDate { get; set; }
        public ScheduleGroupModel Groups { get; set; }
        public string SearchTerm { get; set; }
    }
}