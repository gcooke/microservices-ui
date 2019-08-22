using Absa.Cib.MIT.TaskScheduling.Models;
using Gateway.Web.Enums;
using Gateway.Web.Services.Schedule.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        public List<TradeSourceParameter> TradeSources { get; set; }

        public List<Header> Properties { get; set; }

        public IList<SelectListItem> Types { get; set; }

        [Required]
        [Display(Name = "Trade Source Types")]
        [StringLength(100, ErrorMessage = "Trade Source Type cannot be longer than 100 characters.")]
        public string TradeSourceType { get; set; }

        public ScheduleBatchModel()
        {
            Types = new List<SelectListItem>();
            ConfigurationIdList = new List<string>();
            TradeSources = new List<TradeSourceParameter>();
            Properties = new List<Header>();

            foreach (var index in Enumerable.Range(0, 100))
            {
                TradeSources.Add(new TradeSourceParameter(null, null, null, false, false)
                {
                    MarketDataMap = "Default"
                });
                Properties.Add(new Header(null, null));
            }
        }

        public IList<SelectListItem> TradeSourceTypes
        {
            get
            {
                var values = Enum.GetValues(typeof(TradeSourceType)).Cast<TradeSourceType>();
                return values.Select(x => new SelectListItem { Value = x.ToString(), Text = x.ToString() }).ToList();
            }
        }
    }

    public class ScheduleExecutableModel : BaseScheduleModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Path to Executable")]
        public string PathToExe { get; set; }

        [Display(Name = "Command Line Arguments")]
        public string Arguments { get; set; }

        public long? ExecutableConfigurationId { get; set; }
    }
}