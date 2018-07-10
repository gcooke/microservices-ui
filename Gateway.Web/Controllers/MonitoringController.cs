using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;
using Gateway.Web.Models.Monitoring;
using Gateway.Web.Services.Monitoring.RiskReports;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class MonitoringController : BaseController
    {
        private readonly IRiskReportMonitoringService _riskReportMonitoringService;

        public MonitoringController(ILoggingService loggingService, IRiskReportMonitoringService riskReportMonitoringService) : base(loggingService)
        {
            _riskReportMonitoringService = riskReportMonitoringService;
        }

        public ActionResult Index()
        {
            return View("Index");
        }

        public ActionResult RiskReports(string date = "", bool refresh = false)
        {
            var businessDate = !string.IsNullOrWhiteSpace(date) ?
                DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture) :
                DateTime.Now.AddDays(-1);
            var metrics = _riskReportMonitoringService.GetMetricsForRiskReports(businessDate, refresh);

            var model = new RiskReportMetricsViewModel
            {
                BusinessDate = businessDate,
                Metrics = metrics.ToList()
            };

            return View("RiskReports", model); 
        }
    }
}