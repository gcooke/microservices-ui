using System;
using System.Collections.Generic;

namespace Gateway.Web.Models.Schedule.Output
{
    public class ScheduleStatusSummary
    {
        public IList<ScheduleStatus> TaskStatus { get; set; }
        public IDictionary<string, string> DailySummaries { get; set; }

        public ScheduleStatusSummary()
        {
            TaskStatus = new List<ScheduleStatus>();
            DailySummaries = new Dictionary<string, string>();
        }
    }

    public class ScheduleStatus
    {
        public long? GroupId { get; set; }
        public long ScheduleId { get; set; }
        public string Status { get; set; }
        public Guid? RequestId { get; set; }
        public string StartedAt { get; set; }
        public string FinishedAt { get; set; }
        public int Retries { get; set; }
        public string TimeTakenFormatted { get; set; }
        public string BusinessDate { get; set; }
    }
}