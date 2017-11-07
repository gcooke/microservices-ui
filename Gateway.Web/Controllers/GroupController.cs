﻿using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;
using Gateway.Web.Models.AddIn;
using Gateway.Web.Models.Group;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class GroupController : BaseController
    {
        private readonly IGatewayService _gateway;

        public GroupController(IGatewayService gateway, ILoggingService loggingService)
            : base(loggingService)
        {
            _gateway = gateway;
        }

        public ActionResult Index()
        {
            return Details("0");
        }

        public ActionResult Details(string id)
        {
            var model = _gateway.GetGroup(id.ToLongOrDefault());
            TempData["GroupName"] = model.Name;
            return View("Details", model);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public ActionResult RemoveGroup(string id)
        {
            ModelState.Clear();

            // Validate parameters
            if (string.IsNullOrEmpty(id))
                ModelState.AddModelError("Id", "Id cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = _gateway.DeleteGroup(id.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Security/Groups");

            return Details(id);
        }

        #region ADGroups
        public ActionResult ADGroups(string id)
        {
            var model = _gateway.GetGroupADGroups(id.ToLongOrDefault());
            SetName(model);
            return View("ADGroups", model);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public ActionResult RemoveGroupADGroup(string id, string groupId)
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
                var result = _gateway.DeleteGroupADGroup(id.ToLongOrDefault(), groupId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/Group/ADGroups/{0}", groupId));

            return ADGroups(groupId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult InsertGroupADGroup(FormCollection collection)
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
                var result = _gateway.Create(model);
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/Group/ADGroups/{0}", groupId));

            return ADGroups(groupId);
        }
        #endregion

        #region Permissions
        public ActionResult Permissions(string id)
        {
            var model = _gateway.GetGroupPermisions(id.ToLongOrDefault());
            SetName(model);
            return View("Permissions", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult InsertPermission(FormCollection collection)
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
                var result = _gateway.InsertGroupPermission(groupId.ToLongOrDefault(), permissionId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Group/Permissions/" + groupId);

            return Permissions(groupId);
        }
        #endregion

        #region Portfolios
        public ActionResult Portfolios(string id)
        {
            var model = _gateway.GetGroupPortfolios(id.ToLongOrDefault());
            SetName(model);
            return View("Portfolios", model);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public ActionResult RemovePortfolio(string id, string groupId)
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
                var result = _gateway.DeleteGroupPortfolio(id.ToLongOrDefault(), groupId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/Group/Portfolios/{0}", groupId));

            return Portfolios(groupId);
        }
        #endregion

        #region Sites
        public ActionResult Sites(string id)
        {
            var model = _gateway.GetGroupSites(id.ToLongOrDefault());
            SetName(model);
            return View("Sites", model);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public ActionResult RemoveSite(string id, string groupId)
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
                var result = _gateway.DeleteGroupSite(id.ToLongOrDefault(), groupId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/Group/Sites/{0}", groupId));

            return Sites(groupId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult InsertSite(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var groupId = collection["_id"];
            var siteId = collection["_site"];

            if (string.IsNullOrEmpty(siteId))
                ModelState.AddModelError("Site", "Site cannot be empty");

            if (ModelState.IsValid)
            {
                var result = _gateway.InsertGroupSite(groupId.ToLongOrDefault(), siteId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/Group/Sites/{0}", groupId));

            return Sites(groupId);
        }
        #endregion

        #region Add Ins
        public ActionResult AddIns(string id)
        {
            var model = _gateway.GetGroupAddIns(id.ToLongOrDefault());
            SetName(model);
            return View("AddIns", model);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public ActionResult RemoveAddInVersion(string id, string groupId)
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
                var result = _gateway.DeleteGroupAddInVersion(id.ToLongOrDefault(), groupId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/Group/AddIns/{0}", groupId));

            return AddIns(groupId);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public ActionResult RemoveApplicationVersion(string id, string groupId)
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
                var result = _gateway.DeleteGroupApplicationVersion(id.ToLongOrDefault(), groupId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/Group/AddIns/{0}", groupId));

            return AddIns(groupId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult InsertAddInVersion(FormCollection collection)
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
                var result = _gateway.InsertGroupAddInVersion(groupId.ToLongOrDefault(), addInVersion);
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Group/AddIns/" + groupId);

            return AddIns(groupId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult InsertApplicationVersion(FormCollection collection)
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
                var result = _gateway.InsertGroupApplicationVersion(groupId.ToLongOrDefault(), applicationVersion);
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Group/AddIns/" + groupId);

            return AddIns(groupId);
        }
        #endregion

        #region Users

        public ActionResult Users(string id)
        {
            var model = _gateway.GetGroupUsers(id.ToLongOrDefault());
            SetName(model);
            return View("Users", model);
        }


        #endregion

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public ActionResult RemovePermission(string id, string groupId)
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
                var result = _gateway.DeleteGroupPermission(id.ToLongOrDefault(), groupId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/Group/Permissions/{0}", groupId));

            return Permissions(groupId.ToString());
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