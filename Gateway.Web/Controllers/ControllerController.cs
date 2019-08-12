using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Authorization;
using Gateway.Web.Database;
using Gateway.Web.Models.Controller;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class ControllerController : BaseController
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;
        private readonly ISystemInformation _information;

        public ControllerController(
            IGatewayDatabaseService dataService,
            IGatewayService gateway,
            ILoggingService loggingService,
            ISystemInformation information)
            : base(loggingService)
        {
            _dataService = dataService;
            _gateway = gateway;
            _information = information;
        }

        public ActionResult Dashboard(string id)
        {
            var stats = _dataService.GetResponseStats(DateTime.Today.AddDays(-7));
            var model = new DashboardModel(id);
            model.TotalCalls = stats.GetTotalCalls(id);
            model.TotalErrors = stats.GetTotalErrors(id);
            model.AverageResponse = stats.GetAverageResponse(id);
            return View("Dashboard", model);
        }

        public ActionResult Queues(string id)
        {
            var model = new QueuesModel(id) { Versions = _dataService.GetActiveVersions(id).ToList() };
            foreach (var item in _gateway.GetCurrentQueues(id))
            {
                model.Current.Add(item);
            }
            return View(model);
        }

        public ActionResult Versions(string id, string[] updateResults = null)
        {
            var model = _gateway.GetControllerVersions(id);
            model.UpdateResults = updateResults;

            var versionStatuses = _dataService.GetVersionStatuses().ToArray();

            foreach (var version in model.Versions)
            {
                version.ApplicableStatuses.AddRange(versionStatuses.Select(status =>
                    new SelectListItem
                    {
                        Text = status.Name,
                        Value = status.Name,
                        Selected = version.Status == status.Name
                    }));
            }
            return View("Versions", model);
        }

        public ActionResult Documentation(string id)
        {
            var versions = _gateway.GetControllerVersions(id);
            var model = new DocumentationModel(id);

            var apiModels = GetDocumentationModels(versions, id, _information.GetSetting("ApiDocumentationPath"));
            model.ApiDocumentationModels = apiModels;

            if (id.ToLower() == "pricing")
            {
                var excelModels = GetDocumentationModels(versions, id, _information.GetSetting("ExcelDocumentationPath"));
                model.ExcelDocumentationModels = excelModels;
            }

            return View("Documentation", model);
        }

        private IList<VersionDocumentationModel> GetDocumentationModels(VersionsModel versions, string controller, string path)
        {
            var models = new List<VersionDocumentationModel>();
            foreach (var modelVersion in versions.Versions)
            {
                var documentationPath = string.Format(path, controller, modelVersion.Name);
                var documentationDirectory = new DirectoryInfo(documentationPath);
                var documentationExists = documentationDirectory.Exists;
                models.Add(new VersionDocumentationModel
                {
                    VersionName = modelVersion.Name,
                    HasDocumentation = documentationExists,
                    Status = modelVersion.Status
                });
            }
            return models;
        }

        public ActionResult Generate(string id, string version)
        {
            var result = _gateway.GenerateDocumentation(id, version);
            return Redirect("~/Controller/Documentation/" + id);
        }

        public FileResult Download(string id, string version, string type = "api")
        {
            if (type.ToLower() != "api" && type.ToLower() != "excel")
                type = "api";

            var documentationPath = string.Format(type.ToLower() == "excel" ?
                _information.GetSetting("ExcelDocumentationPath") :
                _information.GetSetting("ApiDocumentationPath"), id, version);

            var documentationDir = new DirectoryInfo(documentationPath);
            var files = documentationDir.GetFiles();

            using (var memoryStream = new MemoryStream())
            {
                using (var ziparchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var fileInfo in files)
                    {
                        var path = Path.Combine(documentationPath, fileInfo.Name);
                        ziparchive.CreateEntryFromFile(path, fileInfo.Name);
                    }
                }
                return File(memoryStream.ToArray(), "application/zip", string.Format("{0} - {1} - {2} Documentation.zip", id, version, type.ToUpper()));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleBasedAuthorize(Roles = "Security.Versions")]
        public ActionResult UpdateVersionStatuses(FormCollection collection)
        {
            var controllerName = collection["_id"];
            var statusUpdates = new List<VersionUpdate>();
            var versionsMarkedForDelete = new List<string>();
            var usedAliases = new List<string>();

            foreach (var key in collection.Keys)
            {
                if (key.ToString() != "_id")
                {
                    var versionName = key.ToString().Split('_')[0];
                    if (key.ToString().Contains("_Delete"))
                    {
                        var markedForDelete = bool.Parse(collection[key.ToString()].Split(',')[0]);
                        if (markedForDelete)
                            statusUpdates.Add(new VersionUpdate(controllerName, versionName, "Deleted", ""));
                    }
                    else if (key.ToString().Contains("_StatusSelection"))
                    {
                        var newStatus = collection[key.ToString()];
                        var alias = collection[versionName + "_Alias"];
                        if (!string.IsNullOrEmpty(alias))
                            usedAliases.AddRange(alias.ToUpper().Split(','));

                        // Only add to collection if the status has changed.
                        if (_dataService.HasStatusChanged(controllerName, versionName, newStatus, alias))
                        {
                            statusUpdates.Add(new VersionUpdate(controllerName, versionName, newStatus, alias));
                        }
                    }
                }
            }

            string[] results;
            if (usedAliases.Count != usedAliases.Distinct().Count())
            {
                results = new[] { "Only a single version can use a specific alias" };
            }
            else
            {
                results = _gateway.UpdateControllerVersionStatuses(statusUpdates);
            }

            //Setup next view
            return Versions(controllerName, results);
        }

        public async Task<ActionResult> Workers(string id)
        {
            var model = new ServiceModel(id);
            model.Services = await _gateway.GetWorkersAsync(id);
            model.Versions = _dataService.GetActiveVersions(id).Select(x => new SelectListItem { Text = x, Value = x }).OrderBy(ord => ord.Text).ToList();
            return View(model);
        }

        public ActionResult History(string id, string sortOrder, string search = null, string searchResultsText = null)
        {
            if (string.IsNullOrEmpty(sortOrder))
            {
                Session.RegisterLastHistoryLocation(Request.Url);
                sortOrder = "time_desc";
            }

            ViewBag.SortColumn = sortOrder;
            ViewBag.SortDirection = sortOrder.EndsWith("_desc") ? "" : "_desc";
            ViewBag.Controller = "Controller";

            var items = _dataService.GetRecentRequests(id, DateTime.Today.AddDays(-7), search);

            var model = new HistoryModel(id);
            model.Requests.AddRange(items, sortOrder);
            model.Requests.SetRelativePercentages();
            model.SearchText = search == null ? null : $"Showing results for '{searchResultsText ?? search}'";
            return View(model);
        }

        public ActionResult Configuration(string id)
        {
            var model = _gateway.GetControllerConfiguration(id);
            return View(model);
        }

        public ActionResult Servers(string id)
        {

            var model = _dataService.GetControllerServers(id);

            return View(model);
        }

        [HttpPost]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult Servers(ControllerServersModel model)
        {
            try
            {
                _dataService.UpdateControllerServers(model);
                return RedirectToAction("Dashboard", new { id = model.Name });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View(model);
        }

        [HttpPost]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult Configuration(ConfigurationModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = _gateway.UpdateControllerConfiguration(model);

                    if (response.Successfull)
                        return RedirectToAction("Dashboard", new { id = model.Name });

                    ModelState.AddModelError(string.Empty, response.Message);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View(model);
        }

        public ActionResult RequestsChart(string id, string date)
        {
            DateTime start;
            if (!DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out start))
                start = DateTime.Today.AddDays(-1);

            var model = _dataService.GetControllerRequestSummary(id, start);
            return View(model);
        }

        public ActionResult TimeChart(string id, string date)
        {
            DateTime start;
            if (!DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out start))
                start = DateTime.Today.AddDays(-1);

            var model = _dataService.GetControllerTimeSummary(id, start);
            return View(model);
        }

        [HttpGet]
        public JsonResult GetHistoricalQueueData(string controllerName, string[] versions)
        {
            var start = DateTime.Now.AddDays(-1);
            var data = _dataService.GetQueueChartModel(start, new List<string>(new[] { controllerName }));

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLiveQueueData(DateTime startDateTime, DateTime? endDateTime, string controllerName, string[] versions)
        {
            var start = DateTime.Now.AddMinutes(-1);
            var data = _dataService.GetQueueChartModel(start, new List<string>(new[] { controllerName }));

            return Json(data.Data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RequestWorkers([System.Web.Http.FromBody]RequestedWorkers requestedWorkers)
        {
            ModelState.Clear();

            await _gateway.RequestWorkersAsync(requestedWorkers);
            return Redirect($"~/Controller/Workers/{requestedWorkers.ControllerName}");
        }
    }
}