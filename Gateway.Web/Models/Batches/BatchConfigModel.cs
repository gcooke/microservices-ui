using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Gateway.Web.Enums;

namespace Gateway.Web.Models.Batches
{
    public class BatchConfigModel
    {
        public long ConfigurationId { get; set; }

        [Display(Name = "Configuration Template")]
        public string ConfigurationTemplate { get; set; }

        [Required]
        [Display(Name = "Risk Batch Type")]
        [StringLength(50, ErrorMessage = "Type cannot be longer than 50 characters.")]
        public string Type { get; set; }

        [Required]
        [Display(Name = "Trade Source Type")]
        [StringLength(100, ErrorMessage = "Trade Source Type cannot be longer than 100 characters.")]
        public string TradeSourceType { get; set; }

        [Required]
        [Display(Name = "Market Data Map")]
        [StringLength(100, ErrorMessage = "Trade Source Type cannot be longer than 100 characters.")]
        public string MarketDataMapName { get; set; }

        [Required]
        [Display(Name = "Output Type")]
        [StringLength(100, ErrorMessage = "Output Type cannot be longer than 100 characters.")]
        public string OutputType { get; set; }

        [Required]
        [Display(Name = "Output Tag")]
        [StringLength(100, ErrorMessage = "Output Type cannot be longer than 100 characters.")]
        public string OutputTag { get; set; }

        [Required]
        [Display(Name = "State Time to Live (Minutes)")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "The State Time to Live (Minutes) field name must contain numbers only.")]
        public int? StateTtlMinutes { get; set; }

        public bool CreateAnother { get; set; }

        public bool IsSelected { get; set; }

        public bool CreateAnotherBasedOnCurrentConfiguration { get; set; }

        public bool IsUpdating => ConfigurationId != 0;

        public int ScheduleCount { get; set; }
        
        public IList<SelectListItem> TradeSourceTypes
        {
            get
            {
                var values = Enum.GetValues(typeof(TradeSourceType)).Cast<TradeSourceType>();
                return values.Select(x => new SelectListItem { Value = x.ToString(), Text = x.ToString() }).ToList();
            }
        }

        public IList<SelectListItem> OutputTypes
        {
            get
            {
                var values = Enum.GetValues(typeof(OutputType)).Cast<OutputType>();
                return values.Select(x => new SelectListItem { Value = x.ToString(), Text = x.ToString() }).ToList();
            }
        }


        public IList<SelectListItem> MarketDataMaps
        {
            get
            {
                var values = Enum.GetValues(typeof(MarketDataMap)).Cast<MarketDataMap>();
                return values.Select(x => new SelectListItem { Value = x.ToString(), Text = x.ToString() }).ToList();
            }
        }

        public IList<SelectListItem> ConfigurationTemplates { get; set; }
    }
}