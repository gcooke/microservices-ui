using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Gateway.Web.Models.Security;
using Gateway.Web.Services.Batches.Interrogation.Models.Enums;

namespace Gateway.Web.Models.Interrogation
{
    public class InterrogationModel
    {
        public InterrogationModel()
        {
            BatchTypes = new List<SelectListItem>();
            TradeSources = new List<SelectListItem>();
            TradeSource = "SOUTH_AFRICA";
            ReportDate = DateTime.Today;
            ReportDateString = ReportDate.ToString("yyyy-MM-dd");
            BatchType = "Counterparty.PFE";
            MinimumLevel = MonitoringLevel.Warning;
            Report = new ReportsModel("Batch Interrogation");
        }

        // Navigation Lookups
        public List<SelectListItem> BatchTypes { get; }
        public List<SelectListItem> TradeSources { get; }

        // Selection
        public string BatchType { get; set; }
        public string TradeSource { get; set; }
        public DateTime ReportDate { get; set; }
        public string ReportDateString { get; set; }
        public MonitoringLevel MinimumLevel { get; set; }

        // Report
        public ReportsModel Report { get; }

        public DateTime ValuationDate
        {
            get
            {
                var date = ReportDate.AddDays(-1);
                while (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
                    date = date.AddDays(-1);
                return date;
            }
        }
    }
}