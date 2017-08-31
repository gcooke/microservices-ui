using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Authorization;
using Gateway.Web.Database;
using Gateway.Web.Models.AddIn;
using Gateway.Web.Models.Controller;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using Controller = System.Web.Mvc.Controller;
using DashboardModel = Gateway.Web.Models.Controllers.DashboardModel;
using HistoryModel = Gateway.Web.Models.Controllers.HistoryModel;
using QueuesModel = Gateway.Web.Models.Controllers.QueuesModel;
using VersionsModel = Gateway.Web.Models.Controllers.VersionsModel;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class ControllersController : Controller
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;
        private readonly IGatewayRestService _gatewayRestService;
        private readonly int _refreshPeriodInSeconds;

        public ControllersController(
            ISystemInformation information,
            IGatewayDatabaseService dataService,
            IGatewayService gateway,
            IGatewayRestService gatewayRestService)
        {
            _dataService = dataService;
            _gateway = gateway;
            _gatewayRestService = gatewayRestService;

            var refreshPeriodInSeconds = information.GetSetting("ControllerActionRefresh");
            if (!int.TryParse(refreshPeriodInSeconds, out _refreshPeriodInSeconds))
            {
                _refreshPeriodInSeconds = 60;
            }
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
            Response.AddHeader("Refresh", _refreshPeriodInSeconds.ToString());
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
            Response.AddHeader("Refresh", _refreshPeriodInSeconds.ToString());
            var model = _gateway.GetWorkers();
            return View(model);
        }

        public ActionResult UsageReport()
        {
            var model = _dataService.GetUsage();
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

        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult Create()
        {
            return View(new ConfigurationModel());
        }

        [HttpPost]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
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

        public ActionResult Versions()
        {
            var model = new VersionsModel();
            return View(model);
        }

        private IEnumerable<AddInVersionModel> GetAddInVersions()
        {
            var response = _gatewayRestService.Get("Security", "addins/versions", CancellationToken.None);

            return response.Successfull
                ? response.Content.GetPayloadAsXElement().Deserialize<IEnumerable<AddInVersionModel>>()
                : null;
        }

        [HttpPost]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult Versions(VersionsModel model)
        {
            var response = _gatewayRestService.Delete("Catalogue", "versions/cleanup", string.Empty,
                CancellationToken.None);

            model.Success = response.Successfull;
            model.Log = response.Content.Message.Split('\n').ToList();
            model.Loading = false;
            model.Loaded = true;
            return View(model);
        }

        public void SomeTaskAsync(int id)
        {
            AsyncManager.OutstandingOperations.Increment();
            Task.Factory.StartNew(taskId =>
            {
                for (int i = 0; i < 100; i++)
                {
                    Thread.Sleep(200);
                    HttpContext.Application["task" + taskId] = i;
                }
                var result = "result";
                AsyncManager.OutstandingOperations.Decrement();
                AsyncManager.Parameters["result"] = result;
                return result;
            }, id);
        }

        public ActionResult SomeTaskCompleted(string result)
        {
            return Content(result, "text/plain");
        }

        public ActionResult SomeTaskProgress(int id)
        {
            return Json(new
            {
                Progress = HttpContext.Application["task" + id]
            }, JsonRequestBehavior.AllowGet);
        }
    }
}