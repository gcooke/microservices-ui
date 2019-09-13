using System;
using System.Threading.Tasks;

namespace Gateway.Web.Services.PortfolioProfile
{
    public interface IPortfolioProfileMonitoringService
    {
        PortfolioProfileReportStates GetStatus(string site, DateTime valuationDate, string portfolios);

        PortfolioProfileReport GetReport(string site, DateTime valuationDate, string portfolio, string report);

        Task<bool> Regenerate(string site, DateTime valuationDate, string portfolio, string report, string user);
    }
}