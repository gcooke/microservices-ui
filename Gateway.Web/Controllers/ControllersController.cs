using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Services;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    public class ControllersController : Controller
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;

        public ControllersController(IGatewayDatabaseService dataService, IGatewayService gateway)
        {
            _dataService = dataService;
            _gateway = gateway;
        }

        public ActionResult Dashboard()
        {
            var start = DateTime.Today.AddDays(-7);
            var controllers = _dataService.GetControllerStatistics(start);
            var model = new DashboardModel();
            model.Controllers.AddRange(controllers.OrderBy(c => c.Name));
            return View(model);
        }

        public ActionResult Servers()
        {
            var model = _gateway.GetServers();
            return View(model);
        }

        public ActionResult History()
        {
            var items = _dataService.GetRecentRequests(DateTime.Today.AddDays(-30));
            var model = new HistoryModel();
            model.Requests.AddRange(items);
            return View(model);
        }

        public ActionResult Queues()
        {
            var model = new QueuesModel();
            model.Queues = _dataService.GetControllerQueueSummary(model.HistoryStartTime);
            foreach (var item in _gateway.GetCurrentQueues())
            {
                model.Current.Add(item);
            }
            return View(model);
        }

        public ActionResult Workers()
        {
            var model = _gateway.GetWorkers();
            return View(model);
        }

        public ActionResult QueueChart(string date)
        {
            DateTime start;
            if (!DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out start))
                start = DateTime.Today.AddDays(-1);

            var model = _dataService.GetControllerQueueSummary(start);
            return View(model);
        }
    }
}