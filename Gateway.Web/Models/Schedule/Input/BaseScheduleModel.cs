using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Gateway.Web.Models.Schedule.Input
{
    public abstract class BaseScheduleModel
    {
        public long ScheduleId { get; set; }

        [Display(Name = "Group")]
        public string Group { get; set; }

        [Display(Name = "Group Name")]
        public string GroupName { get; set; }

        [Display(Name = "Parent")]
        public string Parent { get; set; }

        [Display(Name = "Children")]
        public IList<string> Children { get; set; }

        public bool IsUpdating => ScheduleId != 0;

        public IList<SelectListItem> Groups { get; set; }

        public IList<SelectListItem> Parents { get; set; }

        public IList<SelectListItem> ChildSchedules { get; set; }

        public string ScheduleKey { get; set; }

        public bool BulkUpdate { get; set; }

        public string DisplayName { get; set; }

        protected BaseScheduleModel()
        {
            Children = new List<string>();
        }
    }
}