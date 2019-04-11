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

        public ActionResult Index()
        {
            //var model = new InterrogationModel();
            //_service.PopulateLookups(model);
            //return View("RiskBatch", model);

            return Details("SOUTH_AFRICA", "Counterparty.PFE", DateTime.Today.ToString("yyyy-MM-dd"));
        }

        public ActionResult Details(string tradeSource, string batch, string date)
        {
            var model = new InterrogationModel();
            model.TradeSource = tradeSource;
            model.BatchType = batch;
            model.ReportDate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.CurrentUICulture);

            _service.PopulateLookups(model);
            _service.Analyze(model);

            return View("RiskBatch", model);
        }
    }
}