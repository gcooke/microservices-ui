using System;

namespace Gateway.Web.Models.Schedule.Output
{
    public class CustomRunTask
    {
        public CustomRunTask(ScheduleTask schedule, DateTime businessDate)
        {
            ScheduleId = schedule.ScheduleId;
            BusinessDate = businessDate;
            Name = schedule.Name;
            Custom = string.Empty;
        }

        public long ScheduleId { get; set; }

        public string Name { get; set; }

        public DateTime BusinessDate { get; set; }

        public string Custom { get; set; }
    }
}