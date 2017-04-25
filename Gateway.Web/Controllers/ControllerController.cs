using System;
using System.Globalization;
using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models.Controller;
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

        public ActionResult Dashboard(string id)
        {
            var stats = _dataService.GetResponseStats(DateTime.Today.AddDays(-7));
            var model = new DashboardModel(id);
            model.TotalCalls = stats.GetTotalCalls(id);
            model.TotalErrors = stats.GetTotalErrors(id);
            model.AverageResponse = stats.GetAverageResponse(id);
            return View(model);
        }

        public ActionResult Request(string name, string correlationId)
        {
            var model = _dataService.GetRequestDetails(name, correlationId);
            return View(model);
        }

        public ActionResult Queues(string id)
        {
            var model = new QueuesModel(id);
            return View(model);
        }

        public ActionResult Versions(string id)
        {
            var model = new VersionsModel(id);
            return View(model);
        }

        public ActionResult Workers(string id)
        {
            var model = new WorkersModel(id);
            return View(model);
        }

        public ActionResult History(string id)
        {
            var items = _dataService.GetRecentRequests(id, DateTime.Today.AddDays(-7));

            var model = new HistoryModel(id);
            model.Requests.AddRange(items);
            return View(model);
        }

        public ActionResult Configuration(string id)
        {
            var model = new ConfigurationModel(id);
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

        public ActionResult TimeChart(string id, string date)
        {
            DateTime start;
            if (!DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out start))
                start = DateTime.Today.AddDays(-1);

            var model = _dataService.GetControllerTimeSummary(id, start);
            return View(model);
        }
    }
}