using System.Collections.Generic;
using Gateway.Web.Models.Schedule.Input;

namespace Gateway.Web.Models.Schedule.Output
{
    public class ScheduleGroup
    {
        public long GroupId { get; set; }

        public string Schedule { get; set; }

        public string FriendlyScheduleDescription => string.IsNullOrWhiteSpace(Schedule)
            ? string.Empty
            : CronExpressionDescriptor.ExpressionDescriptor.GetDescription(Schedule);

        public string FriendScheduledTime { get; set; }

        public IList<ScheduleTask> Tasks { get; set; }

        public ScheduleBatchModel ScheduleBatchModel { get; set; }

        public ScheduleWebRequestModel ScheduleWebRequestModel { get; set; }

        public ScheduleGroup()
        {
            Tasks = new List<ScheduleTask>();
            ScheduleBatchModel = new ScheduleBatchModel();
            ScheduleWebRequestModel = new ScheduleWebRequestModel();
        }
    }
}