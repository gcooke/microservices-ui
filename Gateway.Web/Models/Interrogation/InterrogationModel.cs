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
            MinimumLevelInput = MinimumLevel.ToString();

            Report = new ReportsModel("Batch Interrogation");
            Tests = new HashSet<string>();
        }

        // Navigation Lookups
        public List<SelectListItem> BatchTypes { get; }
        public List<SelectListItem> TradeSources { get; }

        public List<SelectListItem> MonitoringLevels
        {
            get
            {
                var selectListItems = new List<SelectListItem>();
                var values = Enum.GetValues(typeof(MonitoringLevel));
                foreach (var value in values)
                {
                    selectListItems.Add(new SelectListItem()
                    {
                        Value = value.ToString(),
                        Text = value.ToString()
                    });
                }
                return selectListItems;
            }
        }

        // Selection
        public string BatchType { get; set; }
        public string TradeSource { get; set; }
        public DateTime ReportDate { get; set; }
        public string ReportDateString { get; set; }
        public MonitoringLevel MinimumLevel { get; set; }
        public string MinimumLevelInput { get; set; }

        // Report
        public ReportsModel Report { get; }

        public HashSet<string> Tests { get; }

        public DateTime GetValuationDate()
        {
            return GetPreviousWorkday(ReportDate);
        }

        public DateTime GetPreviousValuationDate()
        {
            var date = GetValuationDate();
            return GetPreviousWorkday(date);
        }

        private DateTime GetPreviousWorkday(DateTime date)
        {
            date = date.AddDays(-1);
            while (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
                date = date.AddDays(-1);
            return date;
        }
    }
}