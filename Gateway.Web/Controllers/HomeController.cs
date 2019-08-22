using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Authorization;
using Gateway.Web.Database;
using Gateway.Web.Models.Home;
using Gateway.Web.Models.Monitoring;
using Gateway.Web.Services.Monitoring.ServerDiagnostics;
using Gateway.Web.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services.Description;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Redis.Caching;
using Gateway.Web.Models.Controllers;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Access")]
    public class HomeController : BaseController
    {
        private readonly IGatewayDatabaseService _dataService;

        public HomeController(IGatewayDatabaseService dataService,
                              ILoggingService loggingService)
            : base(loggingService)
        {
            _dataService = dataService;
        }

        [OutputCache(Duration = 60, VaryByParam = "none")]
        public async Task<ActionResult> Index(string sortOrder)
        {
            var controllers = _dataService.GetControllerNames();
            var model = new IndexModel();
            model.Controllers.AddRange(controllers.OrderBy(c => c));
            return View("Index", model);
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
            while (uri != null && uri == Request.UrlReferrer)
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