using System;
using System.Collections.Generic;
using Gateway.Web.Utils;

namespace Gateway.Web.Models.Schedule.Output
{
    public class ScheduleTask
    {
        public long ScheduleId { get; set; }

        public string ScheduleKey { get; set; }

        public long? ParentId { get; set; }

        public string Status { get; set; }

        public int Retries { get; set; }

        public Guid? RequestId { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? FinishedAt { get; set; }

        public string GroupName { get; set; }

        public string Parent { get; set; }

        public string Schedule { get; set; }

        public long ChildrenCount { get; set; }

        public IDictionary<long, string> Children { get; set; }

        public string Type { get; }

        public bool HasChildren => ChildrenCount > 0;

        public int TimeTakenMs => (int) ((FinishedAt ?? DateTime.MinValue) - (StartedAt ?? DateTime.MinValue)).TotalMilliseconds;

        public string TimeTakenFormatted => TimeTakenMs.FormatTimeTaken();

        public string FriendlyScheduleDescription => string.IsNullOrWhiteSpace(Schedule)
            ? string.Empty
            : CronExpressionDescriptor.ExpressionDescriptor.GetDescription(Schedule);

        public string Name { get; set; }

        public string Key { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsBatch { get; set; }

        public ScheduleTask(string type)
        {
            Children = new Dictionary<long, string>();
            Type = type;
        }
    }
}