using System;
using System.Collections.Generic;

namespace Gateway.Web.Services.PortfolioProfile
{
    public class PortfolioProfileReportStates
    {
        public string Site { get; set; }
        public DateTime BusinessDate { get; set; }
        public IList<PortfolioProfileReportState> States { get; set; }

        public PortfolioProfileReportStates(string site, DateTime businessDate)
        {
            Site = site;
            BusinessDate = businessDate;
            States = new List<PortfolioProfileReportState>();
        }
    }
}