using System;
using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models.AddIn;
using Gateway.Web.Models.User;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using System.Globalization;
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
        public ActionResult Details(long id)
        {
            var model = _gateway.GetUser(id.ToString());
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
        public ActionResult RemoveUser(long id)
        {
            ModelState.Clear();

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = _gateway.RemoveUser(id);
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect("~/Security/Users");

            return Details(id);
        }
        #endregion

        #region Groups
        public ActionResult Groups(long id)
        {
            var model = _gateway.GetUserGroups(id);
            return View("Groups", model);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public ActionResult RemoveGroup(string userId, string groupId)
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
                var result = _gateway.RemoveUserGroup(userId.ToLongOrDefault(), groupId.ToLongOrDefault());

                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/Groups/{0}", userId));

            return Groups(groupId.ToLongOrDefault());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult Groups(FormCollection collection)
        {
            ModelState.Clear();

            // Validate parameters
            var userId = collection["_id"];
            var groupId = collection["_system"];

            if (string.IsNullOrEmpty(groupId))
                ModelState.AddModelError("Group", "Group cannot be empty");

            if (ModelState.IsValid)
            {
                var result = _gateway.InsertUserGroup(userId.ToLongOrDefault(), groupId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/Groups/{0}", userId));

            return Groups(groupId.ToLongOrDefault());
        }
        #endregion

        #region Portfolios
        public ActionResult Portfolios(long id)
        {
            var model = _gateway.GetUserPortfolios(id);
            return View("Portfolios", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult Portfolios(FormCollection collection)
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
                var result = _gateway.InsertUserPortfolio(userId.ToLongOrDefault(), portfolioId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/Portfolios/{0}", userId));

            return Portfolios(userId.ToLongOrDefault());
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public ActionResult RemovePortfolio(string userId, string portfolioId)
        {
            ModelState.Clear();

            if (string.IsNullOrEmpty(portfolioId))
                ModelState.AddModelError("Portfolio", "Portfolio cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = _gateway.RemoveUserPortfolio(userId.ToLongOrDefault(), portfolioId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/Portfolios/{0}", userId));

            return Portfolios(userId.ToLongOrDefault());
        }
        #endregion

        #region Sites
        public ActionResult Sites(string id)
        {
            var model = _gateway.GetUserSites(id.ToLongOrDefault());
            return View("Sites", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult Sites(FormCollection collection)
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
                var result = _gateway.InsertUserSite(userId.ToLongOrDefault(), siteId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/Sites/{0}", userId));

            return Sites(userId);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public ActionResult RemoveSite(string userId, string siteId)
        {
            ModelState.Clear();

            if (string.IsNullOrEmpty(siteId))
                ModelState.AddModelError("Site", "Site cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = _gateway.RemoveUserSite(userId.ToLongOrDefault(), siteId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/Sites/{0}", userId));

            return Sites(userId);
        }
        #endregion

        #region AddIns
        public ActionResult AddIns(string id)
        {
            var model = _gateway.GetUserAddInVersions(id.ToLongOrDefault());
            model.FullName = _usernameService.GetFullName(model.Login);
            return View("AddIns", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult AddIns(FormCollection collection)
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
                var result = _gateway.InsertUserAddInVersions(userId.ToLongOrDefault(), addInVersion);
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/AddIns/{0}", userId));

            return AddIns(userId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult Applications(FormCollection collection)
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
                var result = _gateway.InsertUserApplicationVersions(userId.ToLongOrDefault(), addInVersion);
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/AddIns/{0}", userId));

            return AddIns(userId);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public ActionResult RemoveAddInVersion(string id, string userId)
        {
            ModelState.Clear();

            if (string.IsNullOrEmpty(id))
                ModelState.AddModelError("Id", "Id cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = _gateway.DeleteUserAddInVersions(userId.ToLongOrDefault(), id.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/AddIns/{0}", userId));

            return AddIns(userId);
        }

        [RoleBasedAuthorize(Roles = "Security.Delete")]
        public ActionResult RemoveApplicationVersion(string id, string userId)
        {
            ModelState.Clear();

            if (string.IsNullOrEmpty(id))
                ModelState.AddModelError("Id", "Id cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = _gateway.DeleteUserApplicationVersions(userId.ToLongOrDefault(), id.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //Setup next view
            if (ModelState.IsValid)
                return Redirect(string.Format("~/User/AddIns/{0}", userId));

            return AddIns(userId);
        }
        #endregion

        public ActionResult History(long id, string login, string sortOrder)
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
                user = _gateway.GetUser(id.ToString());
                login = user.Login;
                domain = user.Domain;
                login = user.Domain + "\\" + user.Login;
            }
            else
            {
                //user = _gateway.GetNonUser(login);
                //login = user.Login;
                //login = user.Domain + "\\" + user.Login;
            }

            ViewBag.SortColumn = sortOrder;
            ViewBag.SortDirection = sortOrder.EndsWith("_desc") ? "" : "_desc";
            ViewBag.Controller = "User";

            var items = _dataService.GetRecentUserRequests(login, DateTime.Today.AddDays(-7));

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
            model.Requests.SetRelativePercentages();
            model.Requests.ReplaceResourceNames(_batchNameService);
            return View("History", model);
        }
    }
}