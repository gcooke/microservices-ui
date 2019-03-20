using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;
using Gateway.Web.Database;
using Gateway.Web.Models.Group;
using Gateway.Web.Models.Permission;
using Gateway.Web.Models.Security;
using Gateway.Web.Models.Shared;
using Gateway.Web.Models.User;
using Gateway.Web.Services;
using Gateway.Web.Utils;
namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class SecurityController : BaseController
    {
        private readonly IGatewayService _gateway;
        private readonly IGatewayDatabaseService _database;
        private readonly IManifestService _manifestService;

        public SecurityController(IGatewayService gateway, ILoggingService loggingService, IGatewayDatabaseService database, IManifestService manifestService)
            : base(loggingService)
        {
            _gateway = gateway;
            _database = database;
            _manifestService = manifestService;
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
            return View("Users", model);
        }

        public ActionResult AddIns()
        {
            var model = _gateway.GetApplications();
            return View("Applications", model);
        }

        public ActionResult Permissions()
        {
            var model = _gateway.GetPermissions();
            return View("Permissions", model);
        }

        public ActionResult BusinessFunctions()
        {
            var model = new BusinessFunctionsModel
            {
                BusinessFunctions = _gateway.GetBusinessFunctions().ToList(),
                GroupTypes = _gateway.GetGroupTypes().ToSelectListItems().ToList()
            };
            return View("BusinessFunctions", model);
        }

        public ActionResult GroupTypes()
        {
            var model = new GroupTypesModel
            {
                GroupTypes = _gateway.GetGroupTypes().ToList()
            };
            return View("GroupTypes", model);
        }

        public ActionResult Links()
        {
            var model = _database.GetLinks();
            model.PopulateSelectionLists();
            return View("Links", model);
        }

        public ActionResult Manifest()
        {
            var model = _manifestService.GetReport();
            return View("Manifest", model);
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
            var type = collection["_type"];

            if (string.IsNullOrEmpty(name))
                ModelState.AddModelError("Name", "Name cannot be empty");

            if (string.IsNullOrEmpty(friendly))
                ModelState.AddModelError("Friendly", "Friendly name cannot be empty");

            if (string.IsNullOrEmpty(description))
                ModelState.AddModelError("Description", "Description cannot be empty");

            if (string.IsNullOrEmpty(type))
                ModelState.AddModelError("Type", "Type cannot be empty");

            // Post instruction to security controller
            if (string.Equals("Application", type, StringComparison.CurrentCultureIgnoreCase))
                AddApplication(name, friendly, description);
            else
                AddAddIn(name, friendly, description);

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Security/AddIns/");

            return AddIns();
        }

        private void AddApplication(string name, string friendlyName, string description)
        {
            var model = new ApplicationModel()
            {
                Name = name,
                FriendlyName = friendlyName,
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
        }

        private void AddAddIn(string name, string friendlyName, string description)
        {
            var model = new AddInModel()
            {
                Name = name,
                FriendlyName = friendlyName,
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult InsertUser(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var login = collection["_login"];
            var domain = collection["_domain"];
            var fullName = collection["_fullName"];

            if (string.IsNullOrEmpty(domain))
                ModelState.AddModelError("Domain", "Domain cannot be empty");

            if (string.IsNullOrEmpty(fullName))
                ModelState.AddModelError("_fullName", "Full name cannot be empty");

            if (string.IsNullOrEmpty(login))
                ModelState.AddModelError("_login", "User name cannot be empty");

            // Post instruction to security controller
            var model = new UserModel
            {
                Login = login.ToLower(),
                Domain = domain.ToUpper(),
                FullName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fullName)
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
                return Redirect("~/Security/Users");

            return Users();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult UpgradeAddIn(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var fromVersion = collection["_from"];
            var toVersion = collection["_to"];
            var fromType = StripApplicationType(ref fromVersion);
            var toType = StripApplicationType(ref toVersion);

            if (string.IsNullOrEmpty(fromVersion))
                ModelState.AddModelError("From", "From cannot be empty");

            if (string.IsNullOrEmpty(toVersion))
                ModelState.AddModelError("To", "To cannot be empty");

            if (fromVersion == toVersion)
                ModelState.AddModelError("Versions", "Cannot re-assign the same version");

            if (fromType != toType)
                ModelState.AddModelError("Versions", "Cannot re-assign to a named application");

            if (ModelState.IsValid)
            {
                string[] result;
                if (fromType == "Application")
                    result = _gateway.UpdateAssignedApplicationVersions(fromVersion, toVersion);
                else
                    result = _gateway.UpdateAssignedAddInVersions(fromVersion, toVersion);

                if (result != null)
                {
                    foreach (var item in result)
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
        public ActionResult AddLink(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var link = new LinkModel();
            link.Name = collection["_name"] ?? string.Empty;
            var glyph = collection["_glyph"] ?? string.Empty;

            if (glyph.StartsWith("0x"))
            {
                var value = HexToInt(glyph.Substring(2));
                link.Glyph = value;
            }
            link.Type = collection["_type"] ?? string.Empty;
            link.AdditionalData = collection["_data"] ?? string.Empty;
            link.Permission = collection["_permission"] ?? string.Empty;

            if (string.IsNullOrEmpty(link.Name))
                ModelState.AddModelError("Name", "Name cannot be empty");
            if (string.IsNullOrEmpty(link.Type))
                ModelState.AddModelError("Type", "Type cannot be empty");

            if (ModelState.IsValid)
            {
                _database.AddLink(link);
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Security/Links");

            return Links();
        }

        private int HexToInt(string hexString)
        {
            return int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public ActionResult RemoveLink(string id)
        {
            ModelState.Clear();

            // Validate parameters
            if (string.IsNullOrEmpty(id))
                ModelState.AddModelError("Id", "Id cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                _database.DeleteLink(id.ToLongOrDefault());
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Security/Links");

            return Links();
        }

        private string StripApplicationType(ref string input)
        {
            var index = input.LastIndexOf("|", StringComparison.Ordinal);
            if (index <= 0) return input;

            var result = input.Substring(index + 1);
            input = input.Substring(0, index);
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult InsertBusinessFunction(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var name = collection["_name"];
            var groupTypeId = collection["_groupType"];

            if (string.IsNullOrEmpty(name))
                ModelState.AddModelError("Name", "Name cannot be empty");

            if (string.IsNullOrEmpty(groupTypeId))
                ModelState.AddModelError("GroupType", "Group Type cannot be empty");

            // Post instruction to security controller
            var model = new BusinessFunction
            {
                Name = name,
                GroupTypeId = int.Parse(groupTypeId)
            };

            if (ModelState.IsValid)
            {
                var result = _gateway.Create(model);
                if (result != null)
                {
                    foreach (var item in result)
                        ModelState.AddModelError("Remote", item);
                }

            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Security/BusinessFunctions/");

            return BusinessFunctions();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult InsertGroupType(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var name = collection["_name"];

            if (string.IsNullOrEmpty(name))
                ModelState.AddModelError("Name", "Name cannot be empty");

            // Post instruction to security controller
            var model = new GroupType
            {
                Name = name,
            };

            if (ModelState.IsValid)
            {
                //post to security controller
                var result = _gateway.Create(model);
                if (result != null)
                {
                    foreach (var item in result)
                        ModelState.AddModelError("Remote", item);
                }

            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Security/GroupTypes/");

            return GroupTypes();
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public ActionResult RemoveBusinessFunction(string id)
        {
            ModelState.Clear();

            // Validate parameters
            if (string.IsNullOrEmpty(id))
                ModelState.AddModelError("Id", "Id cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = _gateway.DeleteBusinessFunction(id.ToIntOrDefault());
                if (result != null)
                {
                    foreach (var item in result)
                        ModelState.AddModelError("Remote", item);
                }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Security/BusinessFunctions");

            return BusinessFunctions();
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public ActionResult RemoveGroupType(string id)
        {
            ModelState.Clear();

            // Validate parameters
            if (string.IsNullOrEmpty(id))
                ModelState.AddModelError("Id", "Id cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = _gateway.DeleteGroupType(id.ToIntOrDefault());
                if (result != null)
                {
                    foreach (var item in result)
                        ModelState.AddModelError("Remote", item);
                }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Security/GroupTypes");

            return GroupTypes();
        }
    }
}