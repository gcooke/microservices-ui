using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGatewayDataService _dataService;

        public HomeController(IGatewayDataService dataService)
        {
            _dataService = dataService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Controllers()
        {
            var model = new ControllersModel(_dataService);
            return View(model);
        }

        public ActionResult HowTo()
        {
            return View();
        }

        public ActionResult Consul()
        {
            //ViewBag.Message = "Your consule page.";
            return View();
        }

        public ActionResult ControllerInfo(string id)
        {
            var model = _dataService.GetControllerInfo(id);
            return View(model);
        }
    }
}