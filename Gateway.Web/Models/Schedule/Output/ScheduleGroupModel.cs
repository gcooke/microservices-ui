using System.Collections.Generic;

namespace Gateway.Web.Models.Schedule.Output
{
    public class ScheduleGroupModel
    {
        public IList<ScheduleGroup> Groups { get; set; }

        public string SearchTerm { get; set; }

        public ScheduleGroupModel()
        {
            Groups = new List<ScheduleGroup>();
        }
    }
}