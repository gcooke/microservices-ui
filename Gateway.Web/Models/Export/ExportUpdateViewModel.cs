using Gateway.Web.Enums;
using Gateway.Web.Models.Schedule.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Gateway.Web.Models.Export
{
    public class ExportUpdateViewModel : BaseScheduleModel
    {
        public ExportUpdateViewModel()
        {
            SourceInformation = new SourceInformationViewModel();
            DestinationInformation = new DestinationInfoViewModel();
            StartDateTime = DateTime.Now.Date;
            ExportTypes = Enum.GetNames(typeof(ExportType))
                .Select(x => new SelectListItem { Value = x, Text = x }).ToList();
            var
            Type = ExportTypes.FirstOrDefault();
        }

        [Required]
        [Display(Name = "Export Name")]
        public string Name { get; set; }

        public long ExportId { get; set; }

        [Display(Name = "Type")]
        public string Type { get; set; }

        public IList<SelectListItem> ExportTypes { get; set; }

        public SourceInformationViewModel SourceInformation { get; set; }

        public DestinationInfoViewModel DestinationInformation { get; set; }

        [Required]
        [Display(Name = "Schedule (CRON)")]
        public string Schedule { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDateTime { get; set; }

        [Required]
        [Display(Name = "Disabled")]
        public bool IsDisabled { get; set; }

        [Required]
        [Display(Name = "Success Email Address")]
        public string SuccessEmailAddress { get; set; }

        [Required]
        [Display(Name = "Failure Email Address")]
        public string FailureEmailAddress { get; set; }

        [Required]
        [Display(Name = "Deleted")]
        public bool IsDeleted { get; set; }
    }
}