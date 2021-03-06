﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;
using Gateway.Web.Models.AddIn;
using Gateway.Web.Models.Group;
using Gateway.Web.Models.Request;
using Gateway.Web.Models.Security;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class GroupController : BaseController
    {
        private readonly IGatewayService _gateway;
        private readonly IActiveDirectoryService _adService;

        public GroupController(IGatewayService gateway, IActiveDirectoryService adService, ILoggingService loggingService)
            : base(loggingService)
        {
            _gateway = gateway;
            _adService = adService;
        }

        public async Task<ActionResult> Index()
        {
            return await Details("0");
        }

        public async Task<ActionResult> Details(string id)
        {
            var model = await _gateway.GetGroup(id.ToLongOrDefault());
            TempData["GroupName"] = model.Name;
            return View("Details", new GroupDetailsModel
            {
                Group = model,
                BusinessFunctions = (await _gateway.GetBusinessFunctions()).ToSelectListItems().ToList()
            });
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> RemoveGroup(string id)
        {
            ModelState.Clear();

            // Validate parameters
            if (string.IsNullOrEmpty(id))
                ModelState.AddModelError("Id", "Id cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = await _gateway.DeleteGroup(id.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Security/Groups");

            return await Details(id);
        }

        #region ADGroups
        public async Task<ActionResult> ADGroups(string id)
        {
            var model = await _gateway.GetGroupADGroups(id.ToLongOrDefault());
            SetName(model);
            return View("ADGroups", model);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> RemoveGroupADGroup(string id, string groupId)
        {
            ModelState.Clear();

            // Validate parameters
            if (string.IsNullOrEmpty(id))
                ModelState.AddModelError("Id", "Id cannot be empty");

            if (string.IsNullOrEmpty(groupId))
                ModelState.AddModelError("GroupId", "GroupId cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = await _gateway.DeleteGroupADGroup(id.ToLongOrDefault(), groupId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/Group/ADGroups/{0}", groupId));

            return await ADGroups(groupId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public async Task<ActionResult> InsertGroupADGroup(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var groupId = collection["_id"];
            var domain = collection["_domain"];
            var adGroup = collection["_adgroup"];

            if (string.IsNullOrEmpty(domain))
                ModelState.AddModelError("Domain", "Domain cannot be empty");

            if (string.IsNullOrEmpty(adGroup))
                ModelState.AddModelError("ADGroup", "AD Group cannot be empty");

            // Post instruction to security controller
            var model = new GroupActiveDirectory()
            {
                GroupId = groupId.ToLongOrDefault(),
                Domain = domain,
                Name = adGroup
            };
            if (ModelState.IsValid)
            {
                var result = await _gateway.Create(model);
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/Group/ADGroups/{0}", groupId));

            return await ADGroups(groupId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public async Task<ActionResult> UpdateBusinessFunction(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var businessFunctionId = collection["_businessFunctions"];
            var groupId = collection["_id"];

            if (string.IsNullOrEmpty(groupId))
                ModelState.AddModelError("Group", "Group cannot be empty");

            if (string.IsNullOrEmpty(businessFunctionId))
                ModelState.AddModelError("BusinessFunction", "Business function cannot be empty");

            if (ModelState.IsValid)
            {
                var result = await _gateway.UpdateGroupBusinessFunction(groupId, businessFunctionId);
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            return Redirect(string.Format("~/Group/Details/{0}", groupId));
        }

        #endregion

        #region Permissions
        public async Task<ActionResult> Permissions(string id)
        {
            var model = await _gateway.GetGroupPermisions(id.ToLongOrDefault());
            SetName(model);
            return View("Permissions", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public async Task<ActionResult> InsertPermission(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var groupId = collection["_id"];
            var permissionId = collection["_permission"];

            if (string.IsNullOrEmpty(permissionId))
                ModelState.AddModelError("Permission", "Permission cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = await _gateway.InsertGroupPermission(groupId.ToLongOrDefault(), permissionId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Group/Permissions/" + groupId);

            return await Permissions(groupId);
        }
        #endregion

        #region Portfolios
        public async Task<ActionResult> Portfolios(string id)
        {
            var model = await _gateway.GetGroupPortfolios(id.ToLongOrDefault());
            SetName(model);
            return View("Portfolios", model);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> RemovePortfolio(string id, string groupId)
        {
            ModelState.Clear();

            // Validate parameters
            if (string.IsNullOrEmpty(id))
                ModelState.AddModelError("Id", "Id cannot be empty");

            if (string.IsNullOrEmpty(groupId))
                ModelState.AddModelError("GroupId", "GroupId cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = await _gateway.DeleteGroupPortfolio(id.ToLongOrDefault(), groupId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/Group/Portfolios/{0}", groupId));

            return await Portfolios(groupId);
        }
        #endregion

        #region Sites
        public async Task<ActionResult> Sites(string id)
        {
            var model = await _gateway.GetGroupSites(id.ToLongOrDefault());
            SetName(model);
            return View("Sites", model);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> RemoveSite(string id, string groupId)
        {
            ModelState.Clear();

            // Validate parameters
            if (string.IsNullOrEmpty(id))
                ModelState.AddModelError("Id", "Id cannot be empty");

            if (string.IsNullOrEmpty(groupId))
                ModelState.AddModelError("GroupId", "GroupId cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = await _gateway.DeleteGroupSite(id.ToLongOrDefault(), groupId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/Group/Sites/{0}", groupId));

            return await Sites(groupId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public async Task<ActionResult> InsertSite(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var groupId = collection["_id"];
            var siteId = collection["_site"];

            if (string.IsNullOrEmpty(siteId))
                ModelState.AddModelError("Site", "Site cannot be empty");

            if (ModelState.IsValid)
            {
                var result = await _gateway.InsertGroupSite(groupId.ToLongOrDefault(), siteId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/Group/Sites/{0}", groupId));

            return await Sites(groupId);
        }
        #endregion

        #region Add Ins
        public async Task<ActionResult> AddIns(string id)
        {
            var model = await _gateway.GetGroupAddIns(id.ToLongOrDefault());
            SetName(model);
            return View("AddIns", model);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> RemoveAddInVersion(string id, string groupId)
        {
            ModelState.Clear();

            // Validate parameters
            if (string.IsNullOrEmpty(id))
                ModelState.AddModelError("Id", "Id cannot be empty");

            if (string.IsNullOrEmpty(groupId))
                ModelState.AddModelError("GroupId", "GroupId cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = await _gateway.DeleteGroupAddInVersion(id.ToLongOrDefault(), groupId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/Group/AddIns/{0}", groupId));

            return await AddIns(groupId);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> RemoveApplicationVersion(string id, string groupId)
        {
            ModelState.Clear();

            // Validate parameters
            if (string.IsNullOrEmpty(id))
                ModelState.AddModelError("Id", "Id cannot be empty");

            if (string.IsNullOrEmpty(groupId))
                ModelState.AddModelError("GroupId", "GroupId cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = await _gateway.DeleteGroupApplicationVersion(id.ToLongOrDefault(), groupId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/Group/AddIns/{0}", groupId));

            return await AddIns(groupId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public async Task<ActionResult> InsertAddInVersion(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var groupId = collection["_id"];
            var version = collection["_version"];

            if (string.IsNullOrEmpty(version))
                ModelState.AddModelError("Version", "Version cannot be empty");

            var addInName = version.Split('|')[0];
            var versionName = version.Split('|')[1];

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var addInVersion = new AddInVersionModel() { AddIn = addInName, Version = versionName };
                var result = await _gateway.InsertGroupAddInVersion(groupId.ToLongOrDefault(), addInVersion);
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Group/AddIns/" + groupId);

            return await AddIns(groupId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public async Task<ActionResult> InsertApplicationVersion(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var groupId = collection["_id"];
            var version = collection["_version"];

            if (string.IsNullOrEmpty(version))
                ModelState.AddModelError("Version", "Version cannot be empty");

            var applicationName = version.Split('|')[0];
            var versionName = version.Split('|')[1];

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var applicationVersion = new ApplicationVersionModel() { Application = applicationName, Version = versionName };
                var result = await _gateway.InsertGroupApplicationVersion(groupId.ToLongOrDefault(), applicationVersion);
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Group/AddIns/" + groupId);

            return await AddIns(groupId);
        }
        #endregion

        #region Users

        public async Task<ActionResult> Users(string id)
        {
            var model = await _gateway.GetGroupUsers(id.ToLongOrDefault());
            SetName(model);
            return View("Users", model);
        }


        #endregion

        public async Task<ActionResult> ViewUsers(long groupId)
        {
            var group = await _gateway.GetGroup(groupId);
            var users = await _gateway.GetGroupUsers(groupId);
            var adgroups = await _gateway.GetGroupADGroups(groupId);
            var adUsers = _adService.GetUsers(adgroups);

            var items = users.Items.Concat(adUsers.Items).OrderByDescending(u => u.FullName).Distinct();
            var builder = new CubeBuilder();
            builder.AddColumn("Name");
            builder.AddColumn("Domain");
            builder.AddColumn("Username");
            var data = builder.Build();
            foreach (var item in items.OrderBy(u => u.FullName))
            {
                data.AddRow(new[]
                {
                    item.FullName, item.Domain, item.Login
                });
            }

            var model = new CubeModel(data, group.Name);
            return View("Cube", model);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> RemovePermission(string id, string groupId)
        {
            ModelState.Clear();

            // Validate parameters
            if (string.IsNullOrEmpty(id))
                ModelState.AddModelError("Id", "Id cannot be empty");

            if (string.IsNullOrEmpty(groupId))
                ModelState.AddModelError("GroupId", "GroupId cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = await _gateway.DeleteGroupPermission(id.ToLongOrDefault(), groupId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/Group/Permissions/{0}", groupId));

            return await Permissions(groupId.ToString());
        }

        private void SetName(IGroupModel model)
        {
            object name;
            if (TempData.TryGetValue("GroupName", out name))
            {
                TempData["GroupName"] = name;
                model.Name = name.ToString();
            }
        }
    }
}