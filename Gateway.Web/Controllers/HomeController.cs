using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;
using Gateway.Web.Database;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Models.Home;
using Gateway.Web.Models.Monitoring;
using Gateway.Web.Services.Monitoring.ServerDiagnostics;
using Gateway.Web.Utils;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Access")]
    public class HomeController : BaseController
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IServerDiagnosticsService _serverDiagnosticsService;

        public HomeController(IGatewayDatabaseService dataService,
            IServerDiagnosticsService serverDiagnosticsService,
            ILoggingService loggingService)
            : base(loggingService)
        {
            _dataService = dataService;
            _serverDiagnosticsService = serverDiagnosticsService;
        }

        public async Task<ActionResult> Index(string sortOrder)
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


            var serverDiagnostics = _serverDiagnosticsService.Get();
            serverDiagnostics = FormatServerDiagnostics(serverDiagnostics);

            var helper = new BatchHelper(_dataService);
            var reportDate = helper.GetPreviousWorkday();
            var batches = await helper.GetRiskBatchReportModel(reportDate);

            var model = new IndexModel();            
            model.Controllers.AddRange(_dataService.GetControllerStates(serverDiagnostics));
            model.Services.AddRange(GetServiceState());
            model.Databases.AddRange(GetDatabaseState());
            model.Batches.AddRange(batches.Items); 
            model.Servers.AddRange(serverDiagnostics.Values);
            
            Response.AddHeader("Refresh", "60");
            return View("Index", model);
        }
        
        public IEnumerable<ServiceState> GetServiceState()
        {
            yield return new ServiceState("Gateway (003)", DateTime.Now, StateItemState.Unknown, "Unknown");
            yield return new ServiceState("Gateway (144)", DateTime.Now, StateItemState.Unknown, "Unknown");
            yield return new ServiceState("ScalingService (144)", DateTime.Now, StateItemState.Unknown, "Unknown");
            yield return new ServiceState("Redis (144)", DateTime.Now, StateItemState.Unknown, "Unknown");
        }

        public IEnumerable<DatabaseState> GetDatabaseState()
        {
            yield return new DatabaseState("Gateway", DateTime.Now, StateItemState.Unknown, "Unknown");
            yield return new DatabaseState("PnRFO", DateTime.Now, StateItemState.Unknown, "Unknown");
        }

        public IDictionary<string, ServerDiagnostics> FormatServerDiagnostics(IDictionary<string, ServerDiagnostics> serverDiagnostics)
        {
            var result = new Dictionary<string, ServerDiagnostics>();
            foreach (var serverDiagnostic in serverDiagnostics)
            {
                if (serverDiagnostic.Value == null)
                {
                    var diagnostics = new ServerDiagnostics()
                    {
                        ServerName = serverDiagnostic.Key,
                        Workers = new List<WorkerStats>(),
                        Requests = new List<RequestStats>(),
                        CpuUtilization = 0,
                        Available = false,
                        MemoryUtilization = 0,
                        Timestamp = DateTime.Now
                    };
                    result.Add(serverDiagnostic.Key, diagnostics);
                    continue;
                }

                result.Add(serverDiagnostic.Key, serverDiagnostic.Value);
            }

            return result;
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