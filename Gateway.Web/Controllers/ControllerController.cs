using System;
using System.Globalization;
using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models.Controller;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using Controller = System.Web.Mvc.Controller;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.DynamicData;

namespace Gateway.Web.Controllers
{
    public class ControllerController : Controller
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;

        public ControllerController(IGatewayDatabaseService dataService, IGatewayService gateway)
        {
            _dataService = dataService;
            _gateway = gateway;
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
            var model = new QueuesModel(id);
            model.Queues = _dataService.GetControllerQueueSummary(id, model.HistoryStartTime);
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

        [HttpPost]
        public ActionResult UpdateVersionStatuses(FormCollection collection)
        {
            var controllerName = collection["id"];
            var statusUpdates = new List<VersionUpdate>();
            var versionsMarkedForDelete = new List<string>();
            var usedAliases = new List<string>();

            foreach (var key in collection.Keys)
            {
                if (key.ToString() != "id")
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

        public ActionResult Workers(string id)
        {
            var model = _gateway.GetWorkers(id);
            return View(model);
        }

        public ActionResult History(string id)
        {
            Session.RegisterLastHistoryLocation(Request.Url);

            var items = _dataService.GetRecentRequests(id, DateTime.Today.AddDays(-7));

            var model = new HistoryModel(id);
            model.Requests.AddRange(items);
            model.Requests.SetRelativePercentages();
            return View(model);
        }

        public ActionResult Configuration(string id)
        {
            var model =_gateway.GetControllerConfiguration(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult Configuration(ConfigurationModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = _gateway.UpdateControllerConfiguration(model);

                    if (response.Successfull)
                        return Dashboard(model.Name);

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

        public ActionResult QueueChart(string id, string date)
        {
            DateTime start;
            if (!DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out start))
                start = DateTime.Today.AddDays(-1);

            var model = _dataService.GetControllerQueueSummary(id, start);
            return View(model);
        }
    }
}