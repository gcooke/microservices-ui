using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models.Security;
using Gateway.Web.Services;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    public class PermissionController : Controller
    {
        private readonly IGatewayService _gateway;

        public PermissionController(IGatewayService gateway)
        {
            _gateway = gateway;
        }

        public ActionResult Index()
        {
            return Details(0);
        }

        public ActionResult Details(long id)
        {
            var model = _gateway.GetPermission(id);
            return View("Details", model);
        }        
    }
}