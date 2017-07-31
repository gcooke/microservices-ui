using System;
using System.Linq;
using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models;
using Gateway.Web.Models.Home;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;

        public HomeController(IGatewayDatabaseService dataService, IGatewayService gateway)
        {
            _dataService = dataService;
            _gateway = gateway;
        }

        public ActionResult Index(string sortOrder)
        {
            if (string.IsNullOrEmpty(sortOrder))
            {
                Session.RegisterLastHistoryLocation(Request.Url);
                sortOrder = "time_desc";
            }

            ViewBag.SortColumn = sortOrder;
            ViewBag.SortDirection = sortOrder.EndsWith("_desc") ? "" : "_desc";
            ViewBag.Controller = "Home";
            ViewBag.Action = "Index";

            var items = _dataService.GetRecentRequests(DateTime.Today.AddDays(-1));

            var model = new IndexModel();
            model.Requests.AddRange(items.Take(10), sortOrder);
            model.Requests.SetRelativePercentages();
            return View(model);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult HowTo()
        {
            return View();
        }

        public ActionResult Consul()
        {
            //ViewBag.Message = "Your consule page.";
            return View();
        }
        
        [AllowAnonymous]
        public ActionResult LogOff()
        {
            var url = Request.Url.Host;
            var uriBuilder = new UriBuilder(url)
            {
                Scheme = Uri.UriSchemeHttp,
                Port = -1 // default port for scheme
            };

            return Redirect(uriBuilder.Uri.ToString());
        }

        public ActionResult ReturnToHistory()
        {
            var uri = Session.GetLastHistoryLocation();
            while(uri != null && uri == Request.UrlReferrer)
                uri = Session.GetLastHistoryLocation();
            if (uri == null) return ReturnToAllHistory();

            return Redirect(uri.ToString());
        }

        public ActionResult ReturnToAllHistory()
        {
            return Redirect("~/Controllers/History");
        }

        public ActionResult Reporting()
        {
            var path = Request.Url.GetLeftPart(UriPartial.Authority);
            return Redirect(path + "/Reporting");
        }
    }
}