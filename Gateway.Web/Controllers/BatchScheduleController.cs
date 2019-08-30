using Bagl.Cib.MIT.Logging;
using Gateway.Web.Database;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Batches.Interfaces;
using Gateway.Web.Services.Schedule.Interfaces;
using Gateway.Web.Services.Schedule.Utils;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Gateway.Web.Controllers
{
    [RoutePrefix("Schedule/Batch")]
    public class BatchScheduleController : BaseController
    {
        private readonly IBatchConfigDataService _batchConfigDataService;
        private readonly IScheduleDataService _scheduleDataService;
        private readonly IScheduleService<ScheduleBatchModel> _scheduleBatchService;
        private readonly IScheduleGroupService _scheduleGroupService;

        public BatchScheduleController(IScheduleDataService scheduleDataService,
            IBatchConfigDataService batchConfigDataService,
            IScheduleService<ScheduleBatchModel> scheduleBatchService,
            IScheduleService<ScheduleWebRequestModel> scheduleWebRequestService,
            IScheduleGroupService scheduleGroupService,
            ILoggingService loggingService)
           : base(loggingService)
        {
            _scheduleDataService = scheduleDataService;
            _batchConfigDataService = batchConfigDataService;
            _scheduleBatchService = scheduleBatchService;
            _scheduleGroupService = scheduleGroupService;
        }

        [HttpGet]
        [Route("Create")]
        public ActionResult CreateForConfiguration()
        {
            var model = new ScheduleBatchModel();
            model.SetData(_batchConfigDataService);
            return View("Create", model);
        }

        [HttpGet]
        [Route("Create/{id}")]
        public ActionResult CreateForConfiguration(long id)
        {
            var model = new ScheduleBatchModel();
            model.SetData(_batchConfigDataService);
            return View("Create", model);
        }

        [HttpGet]
        [Route("Create/Group/{id}")]
        public ActionResult CreateForGroup(long id)
        {
            var model = new ScheduleBatchModel { Group = id.ToString() };
            model.SetData(_batchConfigDataService);
            return View("Create", model);
        }

        [HttpGet]
        [Route("Create/Bulk")]
        public ActionResult CreateForManyConfiguration(string items)
        {
            var configurationIdList = items.Split(',');
            var model = new ScheduleBatchModel { ConfigurationIdList = configurationIdList.ToList() };
            model.SetData(_batchConfigDataService);

            return View("Create", model);
        }

        [HttpGet]
        [Route("Update/Bulk")]
        public ActionResult UpdateBulk(string items)
        {
            var idList = items.Split(',').Select(long.Parse).ToList();
            var schedules = _scheduleDataService.GetSchedules(idList)
                .Where(x => x.RiskBatchScheduleId != null)
                .ToList();

            var model = schedules.ToBatchInputModel();
            model.SetData(_batchConfigDataService);
            model.BulkUpdate = true;
            return View("Create", model);
        }

        [HttpGet]
        [Route("Update/{id}")]
        public ActionResult Update(long id)
        {
            var schedule = _scheduleDataService.GetSchedule(id);
            var model = schedule.ToBatchInputModel();
            model.SetData(_batchConfigDataService);
            return View("Create", model);
        }

        [HttpPost]
        public ActionResult ValidateCreateOrUpdate(ScheduleBatchModel model)
        {
            var t0ValidationMessage = string.Empty;

            var isT0Valid = IsT0Valid(model, out t0ValidationMessage);

            if (isT0Valid || model.T0ValidationConfirmation)
            {
                ViewBag.T0ValidationMessage = null;
                return CreateOrUpdate(model);
            }

            ViewBag.T0ValidationMessage = t0ValidationMessage;
            model.SetData(_batchConfigDataService);
            return View("Create", model);
        }

        private bool IsT0Valid(ScheduleBatchModel model, out string message)
        {
            message = string.Empty;
            long groupId;

            if (model.TradeSources == null || model.TradeSources.Count == 0)
            {
                return true;
            }

            if (string.IsNullOrEmpty(model.Group) || !long.TryParse(model.Group, out groupId))
            {
                return true;
            }

            var scheduleGroup = _scheduleDataService.GetScheduleGroup(groupId);
            if (scheduleGroup == null)
            {
                return true;
            }

            var hasT0 = model.TradeSources.Any(x => x.IsT0);
            var scheduleTimeT0 = IsScheduledTimeT0(scheduleGroup);

            if (hasT0 && !scheduleTimeT0)
            {
                message =
                    "T0 is enabled for a Trade Source, however the scheduled time is not 8PM or greater. Do you wish to save.";
                return false;
            }

            if (!hasT0 && scheduleTimeT0)
            {
                message =
                    "The scheduled time is 8PM or greater, however T0  is not enabled for any Trade Source. Do you wish to save";
                return false;
            }

            return true;
        }

        private bool IsScheduledTimeT0(ScheduleGroup scheduleGroup)
        {
            if (scheduleGroup != null && !string.IsNullOrWhiteSpace(scheduleGroup.Schedule))
            {
                var scheduleSplit = scheduleGroup.Schedule.Split(' ');

                if (scheduleSplit.Length == 5)
                {
                    var hours = new string[] { "20", "21", "22", "23" };
                    var hourPart = scheduleSplit[1];
                    foreach (var hour in hours)
                    {
                        if (hourPart.Contains(hour))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        [HttpPost]
        public ActionResult CreateOrUpdate(ScheduleBatchModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Group) && string.IsNullOrWhiteSpace(model.GroupName) && string.IsNullOrWhiteSpace(model.Parent))
                    ModelState.AddModelError("Group", "Please ensure either a Group has been selected or a cron schedule for a new group or a parent has been specified");

                if (ModelState.IsValid)
                {
                    if (string.IsNullOrWhiteSpace(model.Parent) && string.IsNullOrWhiteSpace(model.Group))
                    {
                        var id = _scheduleGroupService.CreateOrUpdate(model.GroupName);
                        model.Group = id.ToString();
                    }

                    _scheduleBatchService.ScheduleBatches(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            if (!ModelState.IsValid)
            {
                model.SetData(_batchConfigDataService);
                return View("Create", model);
            }

            return RedirectToAction("Update", "Schedule");
        }

        [HttpGet]
        [Route("{id}/Enable")]
        public ActionResult EnableSchedule(long id)
        {
            var schedule = _scheduleDataService.GetSchedule(id);
            _scheduleBatchService.RescheduleBatches(id, null, schedule.Key);
            return RedirectToAction("Update", "Schedule");
        }
    }
}