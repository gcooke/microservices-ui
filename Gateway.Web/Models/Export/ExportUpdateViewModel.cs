using System;
using System.ComponentModel.DataAnnotations;

namespace Gateway.Web.Models.Export
{
    public class ExportUpdateViewModel
    {
        [Required]
        [Display(Name = "Export Name")]
        public string Name { get; set; }

        public long ExportId { get; set; }

        [Required]
        [Display(Name = "Type")]
        public string Type { get; set; }

        [Required]
        [Display(Name = "Source Information")]
        public string SourceInformation { get; set; }

        [Required]
        [Display(Name = "Destination Information")]
        public string DestinationInformation { get; set; }

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