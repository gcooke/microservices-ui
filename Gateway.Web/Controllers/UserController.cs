using System;
using System.Net.Http;
using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models.AddIn;
using Gateway.Web.Models.Group;
using Gateway.Web.Models.User;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using AddInsModel = Gateway.Web.Models.User.AddInsModel;
using Controller = System.Web.Mvc.Controller;
using PortfoliosModel = Gateway.Web.Models.User.PortfoliosModel;
using SitesModel = Gateway.Web.Models.User.SitesModel;
using System.Globalization;

namespace Gateway.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;

        public UserController(IGatewayDatabaseService dataService, IGatewayService gateway)
        {
            _dataService = dataService;
            _gateway = gateway;
        }

        #region User
        public ActionResult Details(long id)
        {
            var model = _gateway.GetUser(id.ToString());

            return View("Details", model);
        }

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

        [HttpPost]
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
                Login = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(login),
                Domain = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(domain),
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

            return View("Details", model);
        }
        #endregion

        #region Groups
        public ActionResult Groups(long id)
        {
            var model = _gateway.GetUserGroups(id);
            return View(model);
        }

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

            //TODO: Show errors
            return Redirect(string.Format("~/User/Groups/{0}", userId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

            //TODO: Show errors
            return Redirect(string.Format("~/User/Groups/{0}", userId));
        }
        #endregion

        #region Portfolios
        public ActionResult Portfolios(long id)
        {
            var model = _gateway.GetUserPortfolios(id);
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
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

            //TODO: Show errors
            return Redirect(string.Format("~/User/Portfolios/{0}", userId));
        }

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

            //TODO: Show errors
            return Redirect(string.Format("~/User/Portfolios/{0}", userId));
        }
        #endregion

        #region Sites
        public ActionResult Sites(string id)
        {
            var model = _gateway.GetUserSites(id.ToLongOrDefault());
            return View(model);
        }

        [HttpPost]
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

            //TODO: Show errors
            return Redirect(string.Format("~/User/Sites/{0}", userId));
        }

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

            //TODO: Show errors
            return Redirect(string.Format("~/User/Sites/{0}", userId));
        }
        #endregion

        #region AddIns
        public ActionResult AddIns(string id)
        {
            var model = _gateway.GetUserAddInVersions(id.ToLongOrDefault());
            return View(model);
        }

        [HttpPost]
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

            //TODO: Show errors
            return Redirect(string.Format("~/User/AddIns/{0}", userId));
        }

        public ActionResult RemoveAddInVersion(string userId, string addInVersionId)
        {
            ModelState.Clear();

            if (string.IsNullOrEmpty(addInVersionId))
                ModelState.AddModelError("AddIns", "AddIns cannot be empty");

            // Post instruction to security controller
            if (ModelState.IsValid)
            {
                var result = _gateway.DeleteUserAddInVersions(userId.ToLongOrDefault(), addInVersionId.ToLongOrDefault());
                if (result != null)
                    foreach (var item in result)
                    {
                        ModelState.AddModelError("Remote", item);
                    }
            }

            //TODO: Show errors
            return Redirect(string.Format("~/User/AddIns/{0}", userId));
        }
        #endregion

        public ActionResult History(long id, string login, string sortOrder)
        {
            if (string.IsNullOrEmpty(sortOrder))
            {
                Session.RegisterLastHistoryLocation(Request.Url);
                sortOrder = "time_desc";
            }

            var domain = "INTRANET";
            if (id >= 1 && string.IsNullOrEmpty(login))
            {
                var user = _gateway.GetUser(id.ToString());
                domain = user.Domain;
                login = user.Login;
            }
            if (id < 1)
            {
                var user = _gateway.GetUser(login);
                domain = user.Domain;
                id = user.Id;
            }

            ViewBag.SortColumn = sortOrder;
            ViewBag.SortDirection = sortOrder.EndsWith("_desc") ? "" : "_desc";
            ViewBag.Controller = "User";

            var items = _dataService.GetRecentUserRequests(domain + "\\" + login, DateTime.Today.AddDays(-7));

            var model = new HistoryModel(id, login);
            model.Requests.AddRange(items, sortOrder);
            model.Requests.SetRelativePercentages();
            return View(model);
        }
    }
}