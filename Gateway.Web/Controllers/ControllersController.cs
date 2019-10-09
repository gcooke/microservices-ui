using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Xml.Linq;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Authorization;
using Gateway.Web.Database;
using Gateway.Web.Models.AddIn;
using Gateway.Web.Models.Controller;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using DashboardModel = Gateway.Web.Models.Controllers.DashboardModel;
using HistoryModel = Gateway.Web.Models.Controllers.HistoryModel;
using QueuesModel = Gateway.Web.Models.Controllers.QueuesModel;
using VersionsModel = Gateway.Web.Models.Controllers.VersionsModel;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class ControllersController : BaseController
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;
        private readonly IGateway _gatewayRestService;
        private readonly IBatchNameService _batchNameService;
        private readonly IBasicRestService _basicRestService;
        private readonly int _refreshPeriodInSeconds;

        public ControllersController(
            ISystemInformation information,
            IGatewayDatabaseService dataService,
            IGatewayService gateway,
            IGateway gatewayRestService,
            ILoggingService loggingService,
            IBatchNameService batchNameService,
            IBasicRestService basicRestService
            )
            : base(loggingService)
        {
            _dataService = dataService;
            _gateway = gateway;
            _gatewayRestService = gatewayRestService;
            _batchNameService = batchNameService;
            _basicRestService = basicRestService;

            var refreshPeriodInSeconds = information.GetSetting("ControllerActionRefresh");
            if (!int.TryParse(refreshPeriodInSeconds, out _refreshPeriodInSeconds))
            {
                _refreshPeriodInSeconds = 60;
            }
        }

        public ActionResult Dashboard()
        {
            var start = DateTime.Today.AddDays(-1);
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
            model.Requests.ReplaceResourceNames(_batchNameService);
            return View(model);
        }

        public ActionResult Queues()
        {
            var model = new QueuesModel
            {
                Controllers = _dataService.GetControllerNames()
            };

            return View(model);
        }

        public async Task<ActionResult> Workers()
        {
            var model = new ServiceModel("");
            model.Services = await _gateway.GetWorkersAsync();
            return View(model);
        }

        public ActionResult UsageReport()
        {
            var model = _dataService.GetUsage();
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
                        return Redirect($"~/Controller/Servers/{model.Name}");

                    ModelState.AddModelError(string.Empty, response.Message);
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
            var response = _gatewayRestService.Get<XElement>("Security", "addins/versions").Result;

            return response.Successfull
                ? response.Body.Deserialize<IEnumerable<AddInVersionModel>>()
                : null;
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

        public ActionResult Actions()
        {
            return View();
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> KillAll()
        {
            await _gateway.KillWorkersAsync();
            return RedirectToAction("Workers");
        }


        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> ShutdownWorkers(string controllername)
        {
            await _gateway.ShutdownWorkersAsync(controllername);
            return Redirect($"~/Controller/Workers/{controllername}");
        }

        public async Task<ActionResult> ShutdownWorker(string controllername, string queuename, string id)
        {
            await _gateway.ShutdownWorkerAsync(queuename, id);
            return Redirect($"~/Controller/Workers/{controllername}");
        }

        //[RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> KillWorker(string controllername, string queuename, string id)
        {
            await _gateway.KillWorkerAsync(queuename, id);
            return Redirect($"~/Controller/Workers/{controllername}");
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> KillWorkers(string controllername)
        {
            await _gateway.KillWorkersAsync(controllername);
            return Redirect($"~/Controller/Workers/{controllername}");
        }

        [HttpGet]
        public JsonResult CurrentQueueData(string[] controllers)
        {
            var start = DateTime.UtcNow.AddMinutes(-60);
            var data = controllers != null && controllers.Any()
                ? _dataService.GetQueueChartModel(start, controllers.ToList())
                : _dataService.GetQueueChartModel(start);
            
            return Json(data.Data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult HistoricalQueueData(string[] controllers)
        {
            var start = DateTime.UtcNow.AddDays(-1);
            var data = controllers != null && controllers.Any()
                ? _dataService.GetQueueChartModel(start, controllers.ToList())
                : _dataService.GetQueueChartModel(start);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


    }
}