using System.Threading.Tasks;
using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Authorization;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class PermissionController : BaseController
    {
        private readonly IGatewayService _gateway;
        private readonly IGateway _gatewayRestService;

        public PermissionController(IGatewayService gateway,
            IGateway gatewayRestService,
            ILoggingService loggingService)
            : base(loggingService)
        {
            _gatewayRestService = gatewayRestService;
            _gateway = gateway;
        }

        public async Task<ActionResult> Index()
        {
            return await Details(0);
        }

        public async Task<ActionResult> Details(long id)
        {
            var model = await _gateway.GetPermission(id);
            return View("Details", model);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> RemovePermission(long id)
        {
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                var query = string.Format("permissions/{0}", id);
                var response = await _gatewayRestService.Delete<string>("Security", query);
                if (!response.Successfull)
                {
                    ModelState.AddModelError("Remote", response.Message);
                }
            }

            return RedirectToAction("Permissions", "Security");
        }
    }
}