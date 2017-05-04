using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Services;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    public class RequestController : Controller
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;

        public RequestController(IGatewayDatabaseService dataService, IGatewayService gateway)
        {
            _dataService = dataService;
            _gateway = gateway;
        }

        public ActionResult Detail(string correlationId)
        {
            var model = _dataService.GetRequestDetails(correlationId);
            return View(model);
        }
    }
}