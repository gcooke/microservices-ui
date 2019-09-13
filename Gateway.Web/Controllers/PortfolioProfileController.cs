using System;
using System.Web.Mvc;
using System.Web.Routing;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;
using Gateway.Web.Services.PortfolioProfile;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "PortfolioProfile.Monitoring")]
    public class PortfolioProfileController : BaseController
    {
        private readonly ISystemInformation _systemInformation;
        private readonly IPortfolioProfileMonitoringService _portfolioProfileMonitoringService;

        public PortfolioProfileController(ISystemInformation systemInformation,
            IPortfolioProfileMonitoringService portfolioProfileMonitoringService,
            ILoggingService loggingService)
            : base(loggingService)
        {
            _systemInformation = systemInformation;
            _portfolioProfileMonitoringService = portfolioProfileMonitoringService;
        }

        public ActionResult Index(string site = null, DateTime? valuationDate = null)
        {
            site = site ?? "SOUTH_AFRICA";
            valuationDate = valuationDate ?? DateTime.Today.AddDays(-1);
            var portfolios = _systemInformation.GetSetting("PortfolioProfilePortfolios");
            var model = _portfolioProfileMonitoringService.GetStatus(site, valuationDate.Value, portfolios);
            return View("Index", model);
        }

        public ActionResult Report(string site, DateTime valuationDate, string portfolio, string report)
        {
            var model = _portfolioProfileMonitoringService.GetReport(site, valuationDate, portfolio, report);
            return View("Report", model);
        }

        public ActionResult Regenerate(string site, DateTime valuationDate, string portfolio, string report)
        {
            _portfolioProfileMonitoringService.Regenerate(site, valuationDate, portfolio, report, HttpContext.User.Identity.Name);
            var routeValueDictionary = new RouteValueDictionary {{"id", "PortfolioProfile"}};
            return RedirectToAction("History", "Controller", routeValueDictionary);
        }
    }
}