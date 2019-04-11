using System;
using System.Collections.Generic;
using Gateway.Web.Models.Security;
using Gateway.Web.Services.Batches.Interrogation.Models.Enums;

namespace Gateway.Web.Models.Interrogation
{
    public class InterrogationModel
    {
        public InterrogationModel()
        {
            BatchTypes = new List<string>();
            TradeSources = new List<string>();
            TradeSource = "SOUTH_AFRICA";
            ReportDate = DateTime.Today;
            MinimumLevel = MonitoringLevel.Warning;
            Report = new ReportsModel("Batch Interrogation");
        }

        // Navigation Lookups
        public List<string> BatchTypes { get; }
        public List<string> TradeSources { get; }

        // Selection
        public string BatchType { get; set; }
        public string TradeSource { get; set; }
        public DateTime ReportDate { get; set; }
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