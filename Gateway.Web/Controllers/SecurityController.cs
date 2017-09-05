using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;
using Gateway.Web.Models.Group;
using Gateway.Web.Models.Permission;
using Gateway.Web.Models.Shared;
using Gateway.Web.Services;
using Gateway.Web.Utils;
namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class SecurityController : BaseController
    {
        private readonly IGatewayService _gateway;

        public SecurityController(IGatewayService gateway, ILoggingService loggingService) 
            : base(loggingService)
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
            return View("AddIns", model);
        }

        public ActionResult Permissions()
        {
            var model = _gateway.GetPermissions();
            return View("Permissions", model);
        }

        [Route("Security/Reports/{report}")]
        public ActionResult Reports(string report)
        {
            var model = _gateway.GetSecurityReport(report);
            return View(model);
        }

        [Route("Security/Reports/{report}/{*parameter}")]
        public ActionResult Reports(string report, string parameter)
        {
            if (parameter == "null") parameter = null;
            var model = _gateway.GetSecurityReport(report, parameter);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ShowReport(FormCollection collection)
        {
            var name = collection["_name"];
            var parameter = collection["_parameter"];

            var route = string.Format("~/Security/Reports/{0}/{1}", name, parameter);
            return Redirect(route);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult InsertAddIn(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var name = collection["_name"];
            var friendly = collection["_friendly"];
            var description = collection["_description"];

            if (string.IsNullOrEmpty(name))
                ModelState.AddModelError("Name", "Name cannot be empty");

            if (string.IsNullOrEmpty(friendly))
                ModelState.AddModelError("Friendly", "Friendly name cannot be empty");

            if (string.IsNullOrEmpty(description))
                ModelState.AddModelError("Description", "Description cannot be empty");

            // Post instruction to security controller
            var model = new AddInModel()
            {
                Name = name,
                FriendlyName = friendly,
                Description = description
            };
            if (ModelState.IsValid)
            {
                var result = _gateway.Create(model);
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Security/AddIns/");

            return AddIns();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult InsertGroup(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var name = collection["_name"];
            var description = collection["_description"];

            if (string.IsNullOrEmpty(name))
                ModelState.AddModelError("Name", "Name cannot be empty");

            if (string.IsNullOrEmpty(description))
                ModelState.AddModelError("Description", "Description cannot be empty");

            // Post instruction to security controller
            var model = new GroupModel()
            {
                Name = name,
                Description = description
            };
            if (ModelState.IsValid)
            {
                var result = _gateway.Create(model);
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Security/Groups/");

            return Groups();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult InsertPermission(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var name = collection["_name"];
            var systemId = collection["_system"];

            if (string.IsNullOrEmpty(name))
                ModelState.AddModelError("Name", "Name cannot be empty");

            if (string.IsNullOrEmpty(systemId))
                ModelState.AddModelError("System", "System cannot be empty");

            // Post instruction to security controller
            var model = new PermissionModel()
            {
                Name = name,
                SystemNameId = systemId.ToLongOrDefault()
            };
            if (ModelState.IsValid)
            {
                var result = _gateway.Create(model);
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Security/Permissions/");

            return Permissions();
        }
    }
}