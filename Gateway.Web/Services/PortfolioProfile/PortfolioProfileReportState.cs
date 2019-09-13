using System;
using Bagl.Cib.MIT.Cube;

namespace Gateway.Web.Services.PortfolioProfile
{
    public class PortfolioProfileReportState
    {
        public string Portfolio { get; set; }
        public string Report { get; set; }
        public string Batch { get; set; }
        public DateTime? LatestRisksAvailable { get; set; }
        public DateTime? LastGeneratedAt { get; set; }
        public string Status { get; set; }

        public void LoadFromRow(IRow row)
        {
            Portfolio = row.GetStringValue("Portfolio");
            Report = row.GetStringValue("Report");
            Batch = row.GetStringValue("Batch");
            LatestRisksAvailable = row.GetValue<DateTime>("Latest Risks Available");
            LastGeneratedAt = row.GetValue<DateTime>("Last Generated At");
            Status = row.GetStringValue("Status");
        }
    }
}