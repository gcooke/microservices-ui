using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Gateway.Web.Models.Schedule.Input
{
    public class ScheduleBatchModel : BaseScheduleModel
    {
        [Required]
        [Display(Name = "Risk Batch Type(s)")]
        public IList<string> ConfigurationIdList { get; set; }

        [Required]
        [Display(Name = "Trade Sources")]
        public string TradeSources { get; set; }

        public IList<SelectListItem> Types { get; set; }

        public ScheduleBatchModel()
        {
            Types = new List<SelectListItem>();
            ConfigurationIdList = new List<string>();
        }
    }
}