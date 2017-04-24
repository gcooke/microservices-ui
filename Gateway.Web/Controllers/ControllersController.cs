using System;
using System.Globalization;
using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models;
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
            var model = _dataService.GetControllers(start);
            return View(model);
        }
        
        public ActionResult History()
        {
            return View();
        }
        
        public ActionResult Queues()
        {
            return View();
        }

        public ActionResult Servers()
        {
            return View();
        }

        public ActionResult Workers()
        {
            return View();
        }
    }
}