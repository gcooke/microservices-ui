using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models.Security;
using Gateway.Web.Services;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    public class SecurityController : Controller
    {
        private readonly IGatewayService _gateway;

        public SecurityController(IGatewayService gateway)
        {
            _gateway = gateway;
        }

        public ActionResult Index()
        {
            return Groups();
        }

        public ActionResult Groups()
        {
            var model = _gateway.GetGroups();
            return View("Groups", model);
        }

        public ActionResult Users()
        {
            var model = _gateway.GetUsers();
            return View(model);
        }

        public ActionResult AddIns()
        {
            var model = _gateway.GetAddIns();
            return View(model);
        }

        public ActionResult Permissions()
        {
            var model = _gateway.GetPermissions();
            return View(model);
        }

        public ActionResult Reports(string report)
        {
            var model = new ReportsModel(report);
            return View(model);
        }
    }
}