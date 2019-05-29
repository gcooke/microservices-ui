using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Bagl.Cib.MSF.ClientAPI.Model;
using Gateway.Web.Authorization;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Models.Schedule.Output;
using Gateway.Web.Services.Batches.Interfaces;
using Gateway.Web.Services.Schedule.Interfaces;
using Gateway.Web.Services.Schedule.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace Gateway.Web.Controllers
{
    [RoutePrefix("Schedule")]
    public class ScheduleController : BaseController
    {
        private readonly IBatchConfigDataService _batchConfigDataService;
        private readonly IGateway _gateway;
        private readonly IScheduleService<ScheduleBatchModel> _scheduleBatchService;
        private readonly IScheduleDataService _scheduleDataService;
        private readonly IScheduleGroupService _scheduleGroupService;
        private readonly IScheduleService<ScheduleWebRequestModel> _scheduleWebRequestService;

        public ScheduleController(IScheduleDataService scheduleDataService,
            IBatchConfigDataService batchConfigDataService,
            IScheduleService<ScheduleBatchModel> scheduleBatchService,
            IScheduleService<ScheduleWebRequestModel> scheduleWebRequestService,
            IScheduleGroupService scheduleGroupService,
            IGateway gateway,
            ILoggingService loggingService)
            : base(loggingService)
        {
            _scheduleDataService = scheduleDataService;
            _batchConfigDataService = batchConfigDataService;
            _scheduleBatchService = scheduleBatchService;
            _scheduleWebRequestService = scheduleWebRequestService;
            _scheduleGroupService = scheduleGroupService;
            _gateway = gateway;
        }

        [HttpGet]
        public ActionResult CreateGroup(string cron)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _scheduleGroupService.CreateOrUpdate(cron);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Update");
            }

            return RedirectToAction("Update");
        }

        [HttpGet]
        [Route("Delete/Bulk")]
        public RedirectToRouteResult DeleteBulk(string items)
        {
            var itemList = items.Split(',').Select(long.Parse);

            foreach (var id in itemList)
            {
                _scheduleDataService.DeleteSchedule(id);
            }

            return RedirectToAction("Update");
        }

        [HttpGet]
        [Route("Group/{id}/Delete")]
        public RedirectToRouteResult DeleteGroup(long id)
        {
            _scheduleGroupService.Delete(id);
            return RedirectToAction("Update");
        }

        [HttpGet]
        [Route("Group/{id}/Delete/Confirm")]
        public ActionResult DeleteGroupConfirmation(long id)
        {
            var group = _scheduleGroupService.GetGroup(id);
            var model = new ScheduleDeleteModel
            {
                GroupId = id,
                ItemsForDeletion = new List<string>
                {
                    $"Group '{group.FriendlyScheduleDescription}'"
                }
            };
            return View("Delete", model);
        }

        [HttpGet]
        [Route("{id}/Delete")]
        public RedirectToRouteResult DeleteSchedule(long id)
        {
            _scheduleDataService.DeleteSchedule(id);
            return RedirectToAction("Update");
        }

        [HttpGet]
        [Route("Delete/Bulk/Confirmation")]
        public ActionResult DeleteSchedulesBulkConfirmation(string items)
        {
            var itemList = items.Split(',').Select(long.Parse).ToList();
            var schedules = _scheduleDataService.GetScheduleTasks(itemList);
            var model = new ScheduleDeleteModel
            {
                ScheduledIdList = items,
                ItemsForDeletion = schedules.Select(x => $"{x.Name} ({x.GroupName})").ToList()
            };
            return View("Delete", model);
        }

        [HttpGet]
        [Route("{id}/Disable")]
        public ActionResult DisableSchedule(long id)
        {
            _scheduleDataService.DisableSchedule(id);
            return RedirectToAction("Update");
        }

        [HttpGet]
        [Route("Status")]
        public string GetStatus(DateTime? businessDate = null, bool includeDailySummaries = false)
        {
            if (businessDate == null)
                businessDate = DateTime.Now;

            var startDate = businessDate?.AddMinutes(-1);
            var endDate = startDate?.AddHours(24);
            var tasks = _scheduleGroupService.GetScheduleGroups(startDate.Value, endDate.Value, null, true);
            var statuses = tasks
                .SelectMany(x => x.Tasks)
                .Select(x => new ScheduleStatus
                {
                    GroupId = x.GroupId,
                    BusinessDate = x.BusinessDate?.ToString("yyyy-MM-dd"),
                    ScheduleId = x.ScheduleId,
                    Status = x.Status,
                    RequestId = x.RequestId,
                    StartedAt = x.StartedAt?.ToString("HH:mm"),
                    FinishedAt = x.FinishedAt?.ToString("HH:mm"),
                    Retries = x.Retries,
                    TimeTakenFormatted = x.TimeTakenFormatted
                })
                .ToList();

            var model = new ScheduleStatusSummary
            {
                TaskStatus = statuses,
                DailySummaries = includeDailySummaries ? _scheduleDataService.GetDailyStatuses(DateTime.Now) : null
            };

            return JsonConvert.SerializeObject(model);
        }

        [HttpGet]
        [Route("")]
        public ActionResult Index(DateTime? businessDate = null, string searchTerm = null)
        {
            if (businessDate == null)
                businessDate = DateTime.Now;

            var date = businessDate.Value.Date;

            var startDate = date.Date.AddMinutes(-1);
            var endDate = startDate.AddHours(24);
            var scheduleGroups = _scheduleGroupService.GetScheduleGroups(startDate, endDate, searchTerm, false, false);

            var model = new ScheduleViewModel
            {
                Groups = new ScheduleGroupModel
                {
                    Groups = scheduleGroups,
                    SearchTerm = searchTerm
                },
                BusinessDate = date,
                SearchTerm = searchTerm
            };

            return View(model);
        }

        [HttpGet]
        [Route("Group/Rerun/{id}/{businessDate}")]
        public void RerunGroup(long id, DateTime businessDate, string searchTerm)
        {
            _scheduleDataService.RerunTaskGroup(id, businessDate, searchTerm);
        }

        [HttpGet]
        [Route("Rerun/{id}/{businessDate}")]
        public void RerunTask(long id, DateTime businessDate)
        {
            _scheduleDataService.RerunTask(id, businessDate);
        }

        [HttpGet]
        [Route("Rerun/{id}/{businessDate}/SkipChildren")]
        public void RerunTaskSkipChildren(long id, DateTime businessDate)
        {
            _scheduleDataService.RerunTask(id, businessDate, false);
        }

        [HttpGet]
        [Route("RunCustom/{id}/{businessDate}")]
        public ActionResult RunCustom(long id, DateTime businessDate)
        {
            var schedule = _scheduleDataService.GetScheduleTask(id);
            var previousBusinessDay = businessDate.AddDays(-1);
            while (previousBusinessDay.DayOfWeek == DayOfWeek.Saturday ||
                   previousBusinessDay.DayOfWeek == DayOfWeek.Sunday)
            {
                previousBusinessDay = previousBusinessDay.AddDays(-1);
            }

            var model = new CustomRunTask(schedule, previousBusinessDay);
            return View("RunCustom", model);
        }

        [HttpPost]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult RunCustomBatch(FormCollection collection)
        {
            var id = long.Parse(collection["_id"]);
            var valuationDate = DateTime.Parse(collection["_businessDate"]).ToString("yyyy-MM-dd");
            var custom = collection["custom"].Replace(" ", "").Replace(Environment.NewLine, ",").Replace(",,", ",");

            if (string.IsNullOrEmpty(custom))
                throw new InvalidOperationException("Custom run must have custom parameters");

            // Invoke batch run with payload
            var put = new Put("RiskBatch");
            put.Query = $"Batch/Run/{id}/{valuationDate}";
            put.SetBody(custom);
            _gateway.Invoke<string>(put);

            var route = new RouteValueDictionary();
            route.Add("id", "riskbatch");
            return RedirectToAction("History", "Controller", route);
        }

        [HttpGet]
        [Route("Group/Stop/{id}/{businessDate}")]
        public void StopGroup(long id, DateTime businessDate, string searchTerm)
        {
            _scheduleDataService.StopTaskGroup(id, searchTerm);
        }

        [HttpGet]
        [Route("Stop/{id}/{businessDate}")]
        public void StopTask(long id, DateTime businessDate)
        {
            _scheduleDataService.StopTask(id);
        }

        [HttpGet]
        [Route("Update")]
        public ActionResult Update(string searchTerm = null)
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMinutes(-1);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

            var groups = _scheduleGroupService.GetGroups(startDate, endDate, searchTerm);
            var model = new ScheduleGroupModel { Groups = groups, SearchTerm = searchTerm };

            foreach (var group in model.Groups)
            {
                group.ScheduleBatchModel.Group = group.GroupId.ToString();
                group.ScheduleWebRequestModel.Group = group.GroupId.ToString();
                group.ScheduleBatchModel.SetData(_batchConfigDataService);
            }

            return View(model);
        }
    }
}