using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models.Security;
using Gateway.Web.Services;
using Gateway.Web.Utils;
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

        public ActionResult RemovePermission(long id, long groupId)
        {
            ModelState.Clear();

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = _gateway.DeletePermission(id, groupId);
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Security/Permissions");

            return Details(id);
        }
    }
}