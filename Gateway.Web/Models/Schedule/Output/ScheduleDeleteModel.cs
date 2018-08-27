using System.Collections.Generic;

namespace Gateway.Web.Models.Schedule.Output
{
    public class ScheduleDeleteModel
    {
        public long? ScheduleId { get; set; }

        public long? GroupId { get; set; }

        public string ScheduledIdList { get; set; }

        public IList<string> ItemsForDeletion { get; set; }
    }
}