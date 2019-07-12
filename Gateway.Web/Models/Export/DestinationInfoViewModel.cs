using Absa.Cib.MIT.TaskScheduling.Models;
using Absa.Cib.MIT.TaskScheduling.Models.Enum;
using Gateway.Web.Models.Schedule.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Gateway.Web.Models.Export
{
    public class DestinationInfoViewModel : BaseScheduleModel
    {
        public DestinationInfoViewModel()
        {
            Arguments = new List<Argument>();

            foreach (var i in Enumerable.Range(0, 100))
            {
                Arguments.Add(new Argument(null, null, null));
            }
        }

        [Display(Name = "DestinationUrl")]
        public string DestinationUrl { get; set; }

        [Display(Name = "FileName")]
        public string FileName { get; set; }

        public IList<SelectListItem> Verbs => Enum.GetNames(typeof(HttpVerb))
            .Select(x => new SelectListItem { Value = x, Text = x }).ToList();

        public IList<SelectListItem> ArgumentDataTypes => Enum.GetNames(typeof(ArgumentDataTypes))
            .Select(x => new SelectListItem { Value = x, Text = x }).ToList();

        public IList<Argument> Arguments { get; set; }

        public IList<SelectListItem> GetArgumentTypes(string selectedArgumentType = null)
        {
            var items = new List<SelectListItem>();

            if (!string.IsNullOrWhiteSpace(selectedArgumentType))
            {
                foreach (var argumentDataType in ArgumentDataTypes)
                {
                    var isSelected = argumentDataType.Value == selectedArgumentType;
                    items.Add(new SelectListItem
                    {
                        Value = argumentDataType.Value,
                        Text = argumentDataType.Text,
                        Selected = isSelected
                    });
                }
            }
            else
            {
                items.AddRange(ArgumentDataTypes);
            }

            return items;
        }
    }
}