using System;
using System.Globalization;
using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    public class ControllerController : Controller
    {
        private readonly IGatewayDataService _dataService;

        public ControllerController(IGatewayDataService dataService)
        {
            _dataService = dataService;
        }

        public ActionResult Configuration(string id)
        {
            var model = _dataService.GetControllerInfo(id);
            return View(model);
        }

        public ActionResult History(string id)
        {
            var model = _dataService.GetControllerInfo(id);
            return View(model);
        }

        public ActionResult Dashboard(string id)
        {
            var model = _dataService.GetControllerInfo(id);
            return View(model);
        }

        public ActionResult Queues(string id)
        {
            var model = _dataService.GetControllerInfo(id);
            return View(model);
        }

        public ActionResult Versions(string id)
        {
            var model = _dataService.GetControllerInfo(id);
            return View(model);
        }

        public ActionResult Workers(string id)
        {
            var model = _dataService.GetControllerInfo(id);
            return View(model);
        }

        public ActionResult RequestsChart(string id, string date)
        {
            DateTime start;
            if (!DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out start))
                start = DateTime.Today.AddDays(-1);

            var model = _dataService.GetControllerRequestSummary(id, start);
            return View(model);
        }
    }
}