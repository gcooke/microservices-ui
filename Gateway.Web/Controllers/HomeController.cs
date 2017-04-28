using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models;
using Gateway.Web.Models.Home;
using Gateway.Web.Services;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;

        public HomeController(IGatewayDatabaseService dataService, IGatewayService gateway)
        {
            _dataService = dataService;
            _gateway = gateway;
        }

        public ActionResult Index()
        {
            var model = new IndexModel();
            model.Queues = _dataService.GetControllerQueueSummary(model.HistoryStartTime);
            foreach (var item in _gateway.GetCurrentQueues())
            {
                model.Current.Add(item);
            }
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