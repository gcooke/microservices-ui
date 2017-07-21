using System;
using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models.User;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using Controller = System.Web.Mvc.Controller;

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

        public ActionResult Index()
        {
            return Details(string.Empty);
        }

        public ActionResult Details(string id)
        {
            UserModel model;
            if (string.IsNullOrEmpty(id))
                model = new UserModel()
                {
                    Domain = "INTRANET",
                    Login = string.Empty,
                    FullName = string.Empty
                };
            else
                model = _gateway.GetUser(id);

            return View("Details", model);
        }

        public ActionResult History(string id, string sortOrder)
        {
            if (string.IsNullOrEmpty(sortOrder))
            {
                Session.RegisterLastHistoryLocation(Request.Url);
                sortOrder = "time_desc";
            }

            ViewBag.SortColumn = sortOrder;
            ViewBag.SortDirection = sortOrder.EndsWith("_desc") ? "" : "_desc";
            ViewBag.Controller = "User";

            var items = _dataService.GetRecentUserRequests("INTRANET\\" + id, DateTime.Today.AddDays(-7));

            var model = new HistoryModel(id);
            model.Requests.AddRange(items, sortOrder);
            model.Requests.SetRelativePercentages();
            return View(model);
        }

        public ActionResult Groups(string id)
        {
            var model = new GroupsModel(id.ToLongOrDefault());
            return View(model);
        }

        public ActionResult Portfolios(string id)
        {
            var model = new PortfoliosModel(id.ToLongOrDefault());
            return View(model);
        }

        public ActionResult Sites(string id)
        {
            var model = new SitesModel(id.ToLongOrDefault());
            return View(model);
        }

        public ActionResult AddIns(string id)
        {
            var model = new AddInsModel(id.ToLongOrDefault());
            return View(model);
        }
    }
}