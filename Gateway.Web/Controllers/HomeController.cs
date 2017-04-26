using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models;
using Gateway.Web.Models.Home;
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
            var model = new IndexModel();
            model.Queues = _dataService.GetControllerQueueSummary(model.HistoryStartTime);
            return View(model);
        }

        public ActionResult About()
        {
            return View();
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
    }
}