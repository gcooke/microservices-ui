using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Absa.Cib.MIT.TaskScheduling.Models;
using Absa.Cib.MIT.TaskScheduling.Models.Enum;

namespace Gateway.Web.Models.Schedule.Input
{
    public class ScheduleWebRequestModel : BaseScheduleModel
    {
        [Required]
        [Display(Name = "Name")]
        [StringLength(80, ErrorMessage = "Name cannot be longer than 80 characters")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Url")]
        [StringLength(500, ErrorMessage = "Url cannot be longer than 500 characters")]
        public string Url { get; set; }

        [Required]
        [Display(Name = "Verb")]
        [StringLength(0, ErrorMessage = "Verb cannot be longer than 10 characters")]
        public string Verb { get; set; }

        [Display(Name = "Payload")]
        public string Payload { get; set; }

        public IList<Argument> Arguments { get; set; }

        public IList<Header> Headers { get; set; }

        public IList<SelectListItem> Verbs => Enum.GetNames(typeof(HttpVerb))
            .Select(x => new SelectListItem {Value = x, Text = x}).ToList();

        public IList<SelectListItem> ArgumentDataTypes => Enum.GetNames(typeof(ArgumentDataTypes))
            .Select(x => new SelectListItem { Value = x, Text = x }).ToList();

        public long? RequestConfigurationId { get; set; }


        public ScheduleWebRequestModel()
        {
            Arguments = new List<Argument>();
            Headers = new List<Header>();

            foreach (var i in Enumerable.Range(0, 100))
            {
                Arguments.Add(new Argument(null, null, null));   
            }

            foreach (var i in Enumerable.Range(0, 100))
            {
                Headers.Add(new Header(null, null));
            }

        }
    }
}