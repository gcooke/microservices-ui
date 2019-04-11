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
            return Details("Counterparty.PFE", DateTime.Today.ToString("yyyy-MM-dd"));
        }

        public ActionResult Details(string batch, string date)
        {
            var actualdate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.CurrentUICulture);
            var model = _service.Analyze(batch, actualdate);            
            return View("RiskBatch", model);
        }
    }
}