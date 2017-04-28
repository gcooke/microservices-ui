using System;
using System.Globalization;
using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models.Reporting;
using Gateway.Web.Services;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    public class ReportingController : Controller
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;

        public ReportingController(IGatewayDatabaseService dataService, IGatewayService gateway)
        {
            _dataService = dataService;
            _gateway = gateway;
        }

        public ActionResult Pfe(string date)
        {
            var model = new PfeModel();
            var report = string.Format("pfe?site=GHANA&partysds=10051752&valDate={0}&dataset=JhbEoDNC", date);
            var element = _gateway.GetReport(report);
            model.ReportXml = element.ToString();
            
            return View(model);
        }
    }
}