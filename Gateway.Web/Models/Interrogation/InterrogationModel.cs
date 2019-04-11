using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Gateway.Web.Models.Security;

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

        // Report
        public ReportsModel Report { get; }
    }
}