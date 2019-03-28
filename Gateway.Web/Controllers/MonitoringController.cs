using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MIT.Redis.Caching;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Authorization;
using Gateway.Web.Database;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class MonitoringController : BaseController
    {
        private readonly IBatchHelper _helper;
        private readonly IGateway _gateway;
        private IRedisCache _cache;

        public MonitoringController(ILoggingService loggingService,
            IBatchHelper helper,
            IGateway gateway,
            IRedisCache cache) : base(loggingService)
        {
            _helper = helper;
            _gateway = gateway;
            _cache = cache;
        }

        public ActionResult Index()
        {
            return View("Index");
        }

        public async Task<ActionResult> RiskBatches(DateTime? businessDate = null)
        {
            var reportDate = businessDate ?? DateTime.Today;
            if (businessDate == null && reportDate.DayOfWeek == DayOfWeek.Monday)
                reportDate = reportDate.AddDays(-2);

            var model = await _helper.GetRiskBatchReportModel(reportDate, "All");
            return View("RiskBatches", model);
        }
    }
}