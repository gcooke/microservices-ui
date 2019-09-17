using CronExpressionDescriptor;
using Gateway.Web.Models.Schedule.Input;
using System;
using System.Collections.Generic;

namespace Gateway.Web.Models.Schedule.Output
{
    public class ScheduleGroup
    {
        public long GroupId { get; set; }

        public string Schedule { get; set; }

        public string FriendlyScheduleDescription => string.IsNullOrWhiteSpace(Schedule)
            ? string.Empty
            : CronExpressionDescriptor.ExpressionDescriptor.GetDescription(Schedule, new Options() { Use24HourTimeFormat = true });

        public string FriendScheduledTime { get; set; }

        public IList<ScheduleTask> Tasks { get; set; }

        public ScheduleBatchModel ScheduleBatchModel { get; set; }

        public ScheduleWebRequestModel ScheduleWebRequestModel { get; set; }

        public DateTime NextOccurrence { get; set; }

        public ScheduleGroup()
        {
            Tasks = new List<ScheduleTask>();
            ScheduleBatchModel = new ScheduleBatchModel();
            ScheduleWebRequestModel = new ScheduleWebRequestModel();
        }
    }
}