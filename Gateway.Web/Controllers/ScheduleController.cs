using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Gateway.Web.Models.Schedule;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Models.Schedule.Output;
using Gateway.Web.Services.Batches.Interfaces;
using Gateway.Web.Services.Schedule.Interfaces;
using Gateway.Web.Services.Schedule.Utils;
using Newtonsoft.Json;

namespace Gateway.Web.Controllers
{
    [RoutePrefix("Schedule")]
    public class ScheduleController : Controller
    {
        private readonly IBatchConfigDataService _batchConfigDataService;
        private readonly IScheduleDataService _scheduleDataService;
        private readonly IScheduleService<ScheduleBatchModel> _scheduleBatchService;
        private readonly IScheduleService<ScheduleWebRequestModel> _scheduleWebRequestService;
        private readonly IScheduleGroupService _scheduleGroupService;

        public ScheduleController(IScheduleDataService scheduleDataService,
            IBatchConfigDataService batchConfigDataService,
            IScheduleService<ScheduleBatchModel> scheduleBatchService,
            IScheduleService<ScheduleWebRequestModel> scheduleWebRequestService,
            IScheduleGroupService scheduleGroupService)
        {
            _scheduleDataService = scheduleDataService;
            _batchConfigDataService = batchConfigDataService;
            _scheduleBatchService = scheduleBatchService;
            _scheduleWebRequestService = scheduleWebRequestService;
            _scheduleGroupService = scheduleGroupService;
        }

        [HttpGet]
        [Route("")]
        public ActionResult Index(DateTime? businessDate = null, string searchTerm = null)
        {
            if (businessDate == null)
                businessDate = DateTime.Now;

            var date = businessDate.Value.Date;

            var scheduleGroups = _scheduleGroupService.GetScheduleGroups(businessDate.Value, searchTerm);

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
        [Route("Update")]
        public ActionResult Update(string searchTerm = null)
        {
            var groups = _scheduleGroupService.GetGroups(searchTerm);
            var model = new ScheduleGroupModel {Groups = groups, SearchTerm = searchTerm};

            foreach (var group in model.Groups)
            {
                group.ScheduleBatchModel.Group = group.GroupId.ToString();
                group.ScheduleWebRequestModel.Group = group.GroupId.ToString();
                group.ScheduleBatchModel.SetData(_batchConfigDataService);
            }

            return View(model);
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
        [Route("{id}/Delete")]
        public RedirectToRouteResult DeleteSchedule(long id)
        {
            _scheduleDataService.DeleteSchedule(id);
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
        [Route("Rerun/{id}/{businessDate}")]
        public void RerunTask(long id, DateTime businessDate)
        {
            _scheduleDataService.RerunTask(id, businessDate);
        }

        [HttpGet]
        [Route("Group/Rerun/{id}/{businessDate}")]
        public void RerunGroup(long id, DateTime businessDate, string searchTerm)
        {
            _scheduleDataService.RerunTaskGroup(id, businessDate, searchTerm);
        }

        [HttpGet]
        [Route("Status")]
        public string GetStatus(DateTime? businessDate = null, bool includeDailySummaries = false)
        {
            if (businessDate == null)
                businessDate = DateTime.Now;

            var tasks = _scheduleGroupService.GetScheduleGroups(businessDate.Value, null, true);
            var statuses = tasks
                .SelectMany(x => x.Tasks)
                .Select(x => new ScheduleStatus
                {
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
    }
}