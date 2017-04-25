using System;
using System.Globalization;
using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models;
using Gateway.Web.Models.Controllers;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    public class ControllersController : Controller
    {
        private readonly IGatewayDataService _dataService;

        public ControllersController(IGatewayDataService dataService)
        {
            _dataService = dataService;
        }

        public ActionResult Dashboard()
        {
            var start = DateTime.Today.AddDays(-7);
            var controllers = _dataService.GetControllerStatistics(start);
            var model = new DashboardModel();
            model.Controllers.AddRange(controllers);
            return View(model);
        }

        public ActionResult Servers()
        {
            var model = new ServersModel();
            model.Servers.Add(new Server() { Name = "JHBPSM020000757" });
            return View(model);
        }

        public ActionResult History()
        {
            var model = new HistoryModel();
            return View(model);
        }

        public ActionResult Queues()
        {
            var model = new QueuesModel();
            return View(model);
        }

        public ActionResult Workers()
        {
            var model = new WorkersModel();
            return View(model);
        }
    }
}