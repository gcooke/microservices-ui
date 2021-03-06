﻿using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Authorization;
using Gateway.Web.Database;
using Gateway.Web.Models.AddIn;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Redis;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Linq;
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
        private readonly IUsernameService _usernameService;
        private readonly IBasicRestService _basicRestService;
        private readonly int _refreshPeriodInSeconds;
        private readonly IRedisService _redisService;

        public ControllersController(
            ISystemInformation information,
            IGatewayDatabaseService dataService,
            IGatewayService gateway,
            IGateway gatewayRestService,
            ILoggingService loggingService,
            IBatchNameService batchNameService,
            IUsernameService usernameService,
            IBasicRestService basicRestService,
            IRedisService redisService
            )
            : base(loggingService)
        {
            _dataService = dataService;
            _gateway = gateway;
            _gatewayRestService = gatewayRestService;
            _batchNameService = batchNameService;
            _usernameService = usernameService;
            _basicRestService = basicRestService;
            _redisService = redisService;

            var refreshPeriodInSeconds = information.GetSetting("ControllerActionRefresh");
            if (!int.TryParse(refreshPeriodInSeconds, out _refreshPeriodInSeconds))
            {
                _refreshPeriodInSeconds = 60;
            }
        }

        public ActionResult Dashboard()
        {
            var start = DateTime.Today.AddDays(-1);
            var controllers = _dataService.GetControllerStatistics(start, string.Empty);
            var model = new DashboardModel();
            model.Controllers.AddRange(controllers.OrderBy(c => c.Name));
            return View(model);
        }

        public ActionResult RedisStats()
        {
            var model = new RedisStatsViewModel()
            {
                ControllerName = Request.QueryString["ControllerName"],
                ControllerVersion = Request.QueryString["ControllerVersionName"],
                MaxPriority = int.Parse(Request.QueryString["MaxPriority"])
            };

            return View(model);
        }

        public string Stats()
        {
            var controllerName = Request.QueryString["ControllerName"];
            var controllerVersion = Request.QueryString["ControllerVersion"];
            var maxPriority = Request.QueryString["MaxPriority"];

            var stats = _redisService.GetRedisStats(controllerName, controllerVersion, int.Parse(maxPriority));

            return JsonConvert.SerializeObject(stats.ToList());
        }

        public async Task AddWorkers()
        {
            var controllerName = Request.QueryString["ControllerName"];
            var controllerVersion = Request.QueryString["ControllerVersion"];
            var priority = Request.QueryString["Priority"];

            await _gateway.RequestWorkersAsync(new RequestedWorkers()
            {
                ControllerName = controllerName,
                Instances = 25,
                Priority = priority,
                Version = controllerVersion
            });
        }

        public string BusyWorkers()
        {
            var controllerName = Request.QueryString["ControllerName"];
            var controllerVersion = Request.QueryString["ControllerVersion"];
            var priority = Request.QueryString["Priority"];

            var correlationIds = _redisService.GetWorkerids(controllerName, controllerVersion, int.Parse(priority));

            return JsonConvert.SerializeObject(correlationIds);
        }

        public async Task<ActionResult> Servers()
        {
            Response.AddHeader("Refresh", _refreshPeriodInSeconds.ToString());
            var model = await _gateway.GetServers();
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
            model.Requests.EnrichHistoryResults(_batchNameService, _usernameService);
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
        public async Task<ActionResult> Create(ConfigurationModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = await _gateway.UpdateControllerConfiguration(model);

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