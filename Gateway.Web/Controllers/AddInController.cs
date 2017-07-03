using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models.Security;
using Gateway.Web.Services;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    public class AddInController : Controller
    {
        private readonly IGatewayService _gateway;

        public AddInController(IGatewayService gateway)
        {
            _gateway = gateway;
        }

        public ActionResult Index()
        {
            return Details(0);
        }

        public ActionResult Details(long id)
        {
            var model = _gateway.GetAddIn(id);
            return View("Details", model);
        }        
    }
}