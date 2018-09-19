using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Batches.Interfaces;
using Gateway.Web.Services.Schedule.Interfaces;
using Gateway.Web.Services.Schedule.Utils;

namespace Gateway.Web.Controllers
{
    [RoutePrefix("Schedule/Batch")]
    public class BatchScheduleController : Controller
    {
        private readonly IBatchConfigDataService _batchConfigDataService;
        private readonly IScheduleDataService _scheduleDataService;
        private readonly IScheduleService<ScheduleBatchModel> _scheduleBatchService;
        private readonly IScheduleGroupService _scheduleGroupService;

        public BatchScheduleController(IScheduleDataService scheduleDataService,
            IBatchConfigDataService batchConfigDataService,
            IScheduleService<ScheduleBatchModel> scheduleBatchService,
            IScheduleService<ScheduleWebRequestModel> scheduleWebRequestService,
            IScheduleGroupService scheduleGroupService)
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
            var model = new ScheduleBatchModel {Group = id.ToString()};
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
                .Where(x => x.RiskBatchConfigurationId != null)
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

            return RedirectToAction("Update","Schedule");
        }

        [HttpPost]
        public ActionResult AssignToGroup()
        {
            try
            {
                if (Request.Form["ConfigurationIdList"] == null)
                    return RedirectToAction("Update", "Schedule");

                var inputModel = new ScheduleBatchModel
                {
                    Group = Request.Form["Group"],
                    Children = new List<string>(),
                    Parent = null,
                    ConfigurationIdList = Request.Form["ConfigurationIdList"].Split(','),
                    TradeSources = Request.Form["TradeSources"]
                };

                _scheduleBatchService.ScheduleBatches(inputModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
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