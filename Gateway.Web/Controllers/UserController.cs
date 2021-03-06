﻿using System;
using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models.AddIn;
using Gateway.Web.Models.User;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using System.Globalization;
using System.Threading.Tasks;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class UserController : BaseController
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;
        private readonly IUsernameService _usernameService;
        private readonly IBatchNameService _batchNameService;

        public UserController(
            IGatewayDatabaseService dataService,
            IGatewayService gateway,
            IUsernameService usernameService,
            IBatchNameService batchNameService,
            ILoggingService loggingService)
            : base(loggingService)
        {
            _dataService = dataService;
            _gateway = gateway;
            _usernameService = usernameService;
            _batchNameService = batchNameService;
        }

        #region User
        public async Task<ActionResult> Details(long id)
        {
            var model = await _gateway.GetUser(id.ToString());
            return View("Details", model);
        }

        [HttpGet]
        [Route("User/NonUserDetails/{domain}/{login}")]
        public ActionResult NonUserDetails(string domain, string login)
        {
            UserModel Nonuser = new UserModel();
            if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(domain))
            {
                Nonuser.Domain = domain;
                Nonuser.Login = login;
                Nonuser.FullName = domain + @"\" + login;
            }

            //Call AD and get domain and details
            //var Nonuser = _gateway.GetNonUser(id);
            Nonuser.FullName = _usernameService.GetFullName(Nonuser.FullName);

            return View("Details", Nonuser);
        }
        
        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> RemoveUser(long id)
        {
            ModelState.Clear();

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = await _gateway.RemoveUser(id);
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Security/Users");

            return await Details(id);
        }
        #endregion

        #region Groups
        public async Task<ActionResult> Groups(long id)
        {
            var model = await _gateway.GetUserGroups(id);
            return View("Groups", model);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> RemoveGroup(string userId, string groupId)
        {
            ModelState.Clear();

            // Validate parameters
            if (string.IsNullOrEmpty(userId))
                ModelState.AddModelError("User", "Unknown user");

            if (string.IsNullOrEmpty(groupId))
                ModelState.AddModelError("Group", "Unknown group");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = await _gateway.RemoveUserGroup(userId.ToLongOrDefault(), groupId.ToLongOrDefault());

                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/Groups/{0}", userId));

            return await Groups(groupId.ToLongOrDefault());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public async Task<ActionResult> Groups(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var userId = collection["_id"];
            var groupId = collection["_system"];

            if (string.IsNullOrEmpty(groupId))
                ModelState.AddModelError("Group", "Group cannot be empty");

            if (ModelState.IsValid)
            {
                var result = await _gateway.InsertUserGroup(userId.ToLongOrDefault(), groupId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/Groups/{0}", userId));

            return await Groups(groupId.ToLongOrDefault());
        }
        #endregion

        #region Portfolios
        public async Task<ActionResult> Portfolios(long id)
        {
            var model = await _gateway.GetUserPortfolios(id);
            return View("Portfolios", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public async Task<ActionResult> Portfolios(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var userId = collection["_id"];
            var portfolioId = collection["_portfolio"];

            if (string.IsNullOrEmpty(portfolioId))
                ModelState.AddModelError("Portfolios", "Portfolio cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = await _gateway.InsertUserPortfolio(userId.ToLongOrDefault(), portfolioId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/Portfolios/{0}", userId));

            return await Portfolios(userId.ToLongOrDefault());
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> RemovePortfolio(string userId, string portfolioId)
        {
            ModelState.Clear();

            if (string.IsNullOrEmpty(portfolioId))
                ModelState.AddModelError("Portfolio", "Portfolio cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = await _gateway.RemoveUserPortfolio(userId.ToLongOrDefault(), portfolioId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/Portfolios/{0}", userId));

            return await Portfolios(userId.ToLongOrDefault());
        }
        #endregion

        #region Sites
        public async Task<ActionResult> Sites(string id)
        {
            var model = await _gateway.GetUserSites(id.ToLongOrDefault());
            return View("Sites", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public async Task<ActionResult> Sites(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var userId = collection["_id"];
            var siteId = collection["_site"];

            if (string.IsNullOrEmpty(siteId))
                ModelState.AddModelError("Sites", "Site cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = await _gateway.InsertUserSite(userId.ToLongOrDefault(), siteId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/Sites/{0}", userId));

            return await Sites(userId);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> RemoveSite(string userId, string siteId)
        {
            ModelState.Clear();

            if (string.IsNullOrEmpty(siteId))
                ModelState.AddModelError("Site", "Site cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = await _gateway.RemoveUserSite(userId.ToLongOrDefault(), siteId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/Sites/{0}", userId));

            return await Sites(userId);
        }
        #endregion

        #region AddIns
        public async Task<ActionResult> AddIns(string id)
        {
            var model = await _gateway.GetUserAddInVersions(id.ToLongOrDefault());
            model.FullName = _usernameService.GetFullName(model.Login);
            return View("AddIns", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public async Task<ActionResult> AddIns(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var userId = collection["_id"];
            var version = collection["_version"];

            if (string.IsNullOrEmpty(version))
                ModelState.AddModelError("Version", "Version cannot be empty");

            var addInName = version.Split('|')[0];
            var versionName = version.Split('|')[1];

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var addInVersion = new AddInVersionModel() { AddIn = addInName, Version = versionName };
                var result = await _gateway.InsertUserAddInVersions(userId.ToLongOrDefault(), addInVersion);
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/AddIns/{0}", userId));

            return await AddIns(userId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public async Task<ActionResult> Applications(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var userId = collection["_id"];
            var version = collection["_version"];

            if (string.IsNullOrEmpty(version))
                ModelState.AddModelError("Version", "Version cannot be empty");

            var application = version.Split('|')[0];
            var versionName = version.Split('|')[1];

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var addInVersion = new ApplicationVersionModel() { Application = application, Version = versionName };
                var result = await _gateway.InsertUserApplicationVersions(userId.ToLongOrDefault(), addInVersion);
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/AddIns/{0}", userId));

            return await AddIns(userId);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> RemoveAddInVersion(string id, string userId)
        {
            ModelState.Clear();

            if (string.IsNullOrEmpty(id))
                ModelState.AddModelError("Id", "Id cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = await _gateway.DeleteUserAddInVersions(userId.ToLongOrDefault(), id.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/AddIns/{0}", userId));

            return await AddIns(userId);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public async Task<ActionResult> RemoveApplicationVersion(string id, string userId)
        {
            ModelState.Clear();

            if (string.IsNullOrEmpty(id))
                ModelState.AddModelError("Id", "Id cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = await _gateway.DeleteUserApplicationVersions(userId.ToLongOrDefault(), id.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/AddIns/{0}", userId));

            return await AddIns(userId);
        }
        #endregion

        public async Task<ActionResult> History(long id, string login, string sortOrder)
        {
            return await HistoryRange(id, login, sortOrder, null, null);
        }

        public async Task<ActionResult> HistoryRange(long id, string login, string sortOrder, string after, string before)
        {
            if (string.IsNullOrEmpty(login))
                throw new Exception("Insufficient details received to resolve login.");

            if (string.IsNullOrEmpty(sortOrder))
            {
                Session.RegisterLastHistoryLocation(Request.Url);
                sortOrder = "time_desc";
            }

            UserModel user;
            string domain = "";

            if (id > 0)
            {
                user = await _gateway.GetUser(id.ToString());
                login = user.Login;
                domain = user.Domain;
                login = user.Domain + "\\" + user.Login;
            }
            else
            {
                //user = await _gateway.GetNonUser(login);
                //login = user.Login;
                //login = user.Domain + "\\" + user.Login;
            }

            DateTime? beforeValue = null;
            DateTime afterValue = DateTime.Today.AddDays(-7);
            if (DateTime.TryParseExact(before, GatewayDatabaseService.UserRequestTimeFormat, CultureInfo.CurrentUICulture, DateTimeStyles.None, out DateTime dt))
                beforeValue = dt;
            if (DateTime.TryParseExact(after, GatewayDatabaseService.UserRequestTimeFormat, CultureInfo.CurrentUICulture, DateTimeStyles.None, out dt))
                afterValue = dt;

            ViewBag.SortColumn = sortOrder;
            ViewBag.SortDirection = sortOrder.EndsWith("_desc") ? "" : "_desc";
            ViewBag.Controller = "User";

            var items = _dataService.GetRecentUserRequests(login, afterValue, beforeValue);

            if (id > 0)
                foreach (var item in items)
                {
                    item.Id = id;
                }

            var fullName = _usernameService.GetFullName(login);
            if (login.Contains("\\"))
            {
                domain = login.Substring(0, (login.IndexOf("\\")));
                login = login.Substring(login.IndexOf("\\") + 1);
            }
            
            var model = new HistoryModel(id, login, domain, fullName);
            model.Requests.AddRange(items, sortOrder);
            model.Requests.EnrichHistoryResults(_batchNameService, _usernameService);
            return View("History", model);
        }

        public async Task<ActionResult> Chart(long id, string login)
        {
            if (string.IsNullOrEmpty(login))
                throw new Exception("Insufficient details received to resolve login.");

            string domain = "";
            ViewBag.Controller = "User";
            var chart = _dataService.GetUserChart(login);

            var fullName = _usernameService.GetFullName(login);
            if (login.Contains("\\"))
            {
                domain = login.Substring(0, (login.IndexOf("\\")));
                login = login.Substring(login.IndexOf("\\") + 1);
            }

            var model = new ChartModel(id, login, domain, fullName);
            model.Chart = chart;
            return View("Chart", model);
        }
    }
}