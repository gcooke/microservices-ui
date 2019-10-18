using System.Threading.Tasks;
using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class AddInController : BaseController
    {
        private readonly IGatewayService _gateway;

        public AddInController(IGatewayService gateway, ILoggingService loggingService)
            : base(loggingService)
        {
            _gateway = gateway;
        }

        public async Task<ActionResult> Index()
        {
            return await Details(0);
        }

        public async Task<ActionResult> Details(long id)
        {
            var model = await _gateway.GetAddIn(id);
            return View("Details", model);
        }

        public async Task<ActionResult> Versions(long id)
        {
            var model = await _gateway.GetAddInVersions();
            return View(model);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> RemoveAddIn(string id)
        {
            ModelState.Clear();

            // Validate parameters
            if (string.IsNullOrEmpty(id))
                ModelState.AddModelError("Id", "Id cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = await _gateway.DeleteAddIn(id.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Security/AddIns");

            return await Details(id.ToLongOrDefault());
        }
    }
}