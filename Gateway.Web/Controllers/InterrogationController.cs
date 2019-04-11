using System;
using System.Globalization;
using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;
using Gateway.Web.Models.Interrogation;
using Gateway.Web.Services;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class InterrogationController : BaseController
    {
        private readonly IRiskBatchInterrogationService _service;

        public InterrogationController(IRiskBatchInterrogationService service, ILoggingService loggingService)
            : base(loggingService)
        {
            _service = service;
        }

        public ActionResult Index(string tradeSource, string batchType, string reportDateString)
        {
            var model = new InterrogationModel();
            _service.PopulateLookups(model);
            model.TradeSource = tradeSource ?? model.TradeSource;
            model.BatchType = batchType ?? model.BatchType;
            model.ReportDateString = reportDateString ?? model.ReportDateString;
            return View("RiskBatch", model);
        }

        public ActionResult Details(string tradeSource, string batchType, string reportDateString)
        {
            var model = new InterrogationModel();
            model.TradeSource = tradeSource;
            model.BatchType = batchType;
            model.ReportDate = DateTime.ParseExact(reportDateString, "yyyy-MM-dd", CultureInfo.CurrentUICulture);

            _service.PopulateLookups(model);
            //_service.Analyze(model);

            return View("RiskBatch", model);
        }
    }
}