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
        private readonly IGatewayRestService _gatewayRestService;

        public PermissionController(IGatewayService gateway,
            IGatewayRestService gatewayRestService,
            ILoggingService loggingService)
            : base(loggingService)
        {
            _gatewayRestService = gatewayRestService;
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

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public ActionResult RemovePermission(long id)
        {
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                var query = string.Format("permissions/{0}", id);
                var response = _gatewayRestService.Delete("Security", "latest", query, string.Empty);
                if (!response.Successfull)
                {
                    ModelState.AddModelError("Remote", response.Content.Message);
                }
            }

            return RedirectToAction("Permissions", "Security");
        }
    }
}