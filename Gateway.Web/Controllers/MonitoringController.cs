using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;
using Gateway.Web.Database;
using Gateway.Web.Models.Monitoring;
using Gateway.Web.Services.Monitoring.RiskReports;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class MonitoringController : BaseController
    {
        private readonly IRiskReportMonitoringService _riskReportMonitoringService;
        private readonly IGatewayDatabaseService _dataService;

        public MonitoringController(ILoggingService loggingService, 
            IRiskReportMonitoringService riskReportMonitoringService,
            IGatewayDatabaseService dataService) : base(loggingService)
        {
            _riskReportMonitoringService = riskReportMonitoringService;
            _dataService = dataService;
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
            var metrics = _riskReportMonitoringService.GetMetricsForRiskReports(businessDate).ToList();

            var groupMetrics = metrics
                .GroupBy(x => new {x.System, x.ReportCategory, x.ReportSubCategory, x.ReportName})
                .Select(x => new GroupedRiskReportMetrics
                {
                    System = x.Key.System,
                    ReportCategory = x.Key.ReportCategory,
                    ReportSubCategory = x.Key.ReportSubCategory,
                    ReportName = x.Key.ReportName,
                    Metrics = x.ToList()
                });

            var model = new RiskReportMetricsViewModel
            {
                BusinessDate = businessDate,
                Metrics = groupMetrics.ToList()
            };

            return View("RiskReports", model); 
        }

        public async Task<ActionResult> RiskBatches()
        {
            var helper = new BatchHelper(_dataService);
            var reportDate = helper.GetPreviousWorkday();
            var batches = await helper.GetRiskBatchReportModel(reportDate);
            return View("RiskBatches", batches);
        }
    }
}