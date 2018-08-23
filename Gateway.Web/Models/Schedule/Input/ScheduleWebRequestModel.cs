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
        public string Name { get; set; }

        [Required]
        [Display(Name = "Url")]
        public string Url { get; set; }

        [Required]
        [Display(Name = "Verb")]
        public string Verb { get; set; }

        [Display(Name = "Payload")]
        [AllowHtml]
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