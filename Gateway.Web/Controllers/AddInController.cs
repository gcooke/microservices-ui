using System.Web.Mvc;
using Gateway.Web.Authorization;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
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

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public ActionResult RemoveAddIn(string id)
        {
            ModelState.Clear();

            // Validate parameters
            if (string.IsNullOrEmpty(id))
                ModelState.AddModelError("Id", "Id cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = _gateway.DeleteAddIn(id.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Security/AddIns");

            return Details(id.ToLongOrDefault());
        }
    }
}