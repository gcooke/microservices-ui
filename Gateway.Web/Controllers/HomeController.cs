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
using System.Threading.Tasks;
using System.Web.Mvc;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Redis.Caching;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Access")]
    public class HomeController : BaseController
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IServerDiagnosticsService _serverDiagnosticsService;
        private readonly IBatchHelper _batchHelper;
        private readonly ISystemInformation _systemInformation;
        private readonly IDatabaseStateProvider _databaseStateProvider;

        public HomeController(IGatewayDatabaseService dataService,
            IServerDiagnosticsService serverDiagnosticsService,
            ILoggingService loggingService,
            IGatewayRestService gateway,
            IBatchHelper batchHelper,
            ISystemInformation systemInformation,
            IDatabaseStateProvider databaseStateProvider)
            : base(loggingService)
        {
            _dataService = dataService;
            _serverDiagnosticsService = serverDiagnosticsService;
            _batchHelper = batchHelper;
            _systemInformation = systemInformation;
            _databaseStateProvider = databaseStateProvider;
        }

        [OutputCache(Duration = 60, VaryByParam = "none")]
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

            var reportDate = _batchHelper.GetPreviousWorkday();
            var batchestask = _batchHelper.GetRiskBatchReportModelAsync(reportDate);


            var servicetask = GetServiceStateAsync();
            var databasetask = GetDatabaseStateAsync();
            var serverDiagnosticstask = _serverDiagnosticsService.GetAsync();

            Task[] tasks = { servicetask, databasetask, serverDiagnosticstask, batchestask };
            Task.WaitAll(tasks);

            var serverDiagnostics = serverDiagnosticstask.Result;

            var controllers = _dataService.GetControllerStates(serverDiagnostics);

            if (serverDiagnostics != null)
                serverDiagnostics = FormatServerDiagnostics(serverDiagnostics);

            var model = new IndexModel();            
            model.Controllers.AddRange(controllers);
            model.Services.AddRange(servicetask.Result);
            model.Databases.AddRange(databasetask.Result);
            model.Batches.AddRange(batchestask.Result.Items);

            if (serverDiagnostics != null)
                model.Servers.AddRange(serverDiagnostics.Values);
            
            Response.AddHeader("Refresh", "60");
            return View("Index", model);
        }
        
        public async Task<List<ServiceState>> GetServiceStateAsync()
        {
            return await Task.Factory.StartNew(() =>
            {
                var servicestates = new List<ServiceState>();
                servicestates.Add(new ServiceState("Gateway (003)", DateTime.Now, StateItemState.Unknown, "Unknown"));
                servicestates.Add(new ServiceState("Gateway (144)", DateTime.Now, StateItemState.Unknown, "Unknown"));
                servicestates.Add(new ServiceState("ScalingService (144)", DateTime.Now, StateItemState.Unknown, "Unknown"));
                servicestates.Add(new ServiceState("Redis (144)", DateTime.Now, StateItemState.Unknown, "Unknown"));

                return servicestates;
            }).ConfigureAwait(false);
        }

        public async Task<List<DatabaseState>> GetDatabaseStateAsync()
        {

            var databases = _systemInformation
                .GetSetting("Databases", "GatewayDatabase;PnRFODatabase;SndTradeDbDatabase;SigmaDatabase").Split(';')
                .ToList();

            return await Task.Factory.StartNew(() =>
            {
                var databaseStates = new List<DatabaseState>();

                foreach (var database in databases)
                {
                    databaseStates.Add(_databaseStateProvider.GetDatabaseState(database));
                }

                return databaseStates;
            }).ConfigureAwait(false);
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

    public interface IDatabaseStateProvider
    {
        DatabaseState GetDatabaseState(string DatabaseConfigId);
    }
}