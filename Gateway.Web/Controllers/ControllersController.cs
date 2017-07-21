using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Database;
using Gateway.Web.Models.Controller;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using Controller = System.Web.Mvc.Controller;
using DashboardModel = Gateway.Web.Models.Controllers.DashboardModel;
using HistoryModel = Gateway.Web.Models.Controllers.HistoryModel;
using QueuesModel = Gateway.Web.Models.Controllers.QueuesModel;

namespace Gateway.Web.Controllers
{
    public class ControllersController : Controller
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;
        private readonly IGatewayRestService _gatewayRestService;

        public ControllersController(
            IGatewayDatabaseService dataService,
            IGatewayService gateway,
            IGatewayRestService gatewayRestService)
        {
            _dataService = dataService;
            _gateway = gateway;
            _gatewayRestService = gatewayRestService;
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

        public ActionResult Aliases()
        {
            var model = _dataService.GetAliases();
            return View("Aliases", model);
        }

        public ActionResult History(string sortOrder)
        {
            if (string.IsNullOrEmpty(sortOrder))
            {
                Session.RegisterLastHistoryLocation(Request.Url);
                sortOrder = "time_desc";
            }

            ViewBag.SortColumn = sortOrder;
            ViewBag.SortDirection = sortOrder.EndsWith("_desc") ? "" : "_desc";
            ViewBag.Controller = "Controllers";

            var items = _dataService.GetRecentRequests(DateTime.Today.AddDays(-30));
            var model = new HistoryModel();
            model.Requests.AddRange(items, sortOrder);
            model.Requests.SetRelativePercentages();
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

        public ActionResult Create()
        {
            return View(new ConfigurationModel());
        }

        public ActionResult ServerInfos()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(ConfigurationModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = _gateway.UpdateControllerConfiguration(model);

                    if (response.Successfull)
                        return RedirectToAction("Dashboard");

                    ModelState.AddModelError(string.Empty, response.Content.Message);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View(model);
        }
    }
}