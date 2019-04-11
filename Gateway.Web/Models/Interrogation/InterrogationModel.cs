using System;
using System.Collections.Generic;
using Gateway.Web.Models.Security;

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
            Report = new ReportsModel("Batch Interrogation");
        }

        // Navigation Lookups
        public List<string> BatchTypes { get; }
        public List<string> TradeSources { get; }

        // Selection
        public string BatchType { get; set; }
        public string TradeSource { get; set; }
        public DateTime ReportDate { get; set; }

        // Report
        public ReportsModel Report { get; }
    }
}