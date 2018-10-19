using System;
using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Batches.Interfaces;
using Gateway.Web.Services.Schedule.Interfaces;
using Gateway.Web.Services.Schedule.Utils;

namespace Gateway.Web.Controllers
{
    [RoutePrefix("Schedule/Executable")]
    public class ExecutableScheduleController : BaseController
    {
        private readonly IBatchConfigDataService _batchConfigDataService;
        private readonly IScheduleDataService _scheduleDataService;
        private readonly IScheduleService<ScheduleExecutableModel> _scheduleService;
        private readonly IScheduleGroupService _scheduleGroupService;

        public ExecutableScheduleController(IScheduleDataService scheduleDataService,
            IBatchConfigDataService batchConfigDataService,
            IScheduleService<ScheduleExecutableModel> scheduleService,
            IScheduleGroupService scheduleGroupService,
            ILoggingService loggingService)
           :base(loggingService)
        {
            _scheduleDataService = scheduleDataService;
            _batchConfigDataService = batchConfigDataService;
            _scheduleService = scheduleService;
            _scheduleGroupService = scheduleGroupService;
        }

        [HttpGet]
        [Route("Create")]
        public ActionResult CreateForConfiguration()
        {
            var model = new ScheduleExecutableModel();
            model.SetData(_batchConfigDataService);
            return View("Create", model);
        }

        [HttpGet]
        [Route("Create/{id}")]
        public ActionResult CreateForConfiguration(long id)
        {
            var model = new ScheduleExecutableModel();
            model.SetData(_batchConfigDataService);
            return View("Create", model);
        }

        [HttpGet]
        [Route("Create/Group/{id}")]
        public ActionResult CreateForGroup(long id)
        {
            var model = new ScheduleExecutableModel { Group = id.ToString()};
            model.SetData(_batchConfigDataService);
            return View("Create", model);
        }

        [HttpGet]
        [Route("Create/Bulk")]
        public ActionResult CreateForManyConfiguration(string items)
        {
            var model = new ScheduleExecutableModel();
            model.SetData(_batchConfigDataService);
            return View("Create", model);
        }


        [HttpGet]
        [Route("Update/{id}")]
        public ActionResult Update(long id)
        {
            var schedule = _scheduleDataService.GetSchedule(id);
            var model = schedule.ToExecutableInputModel();
            model.SetData(_batchConfigDataService);
            return View("Create", model);
        }

        [HttpPost]
        public ActionResult CreateOrUpdate(ScheduleExecutableModel model)
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

                    _scheduleService.ScheduleBatches(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            return RedirectToAction("Update","Schedule");
        }

        [HttpGet]
        [Route("{id}/Enable")]
        public ActionResult EnableSchedule(long id)
        {
            var schedule = _scheduleDataService.GetSchedule(id);
            _scheduleService.RescheduleBatches(id, null, schedule.Key);
            return RedirectToAction("Update", "Schedule");
        }
    }
}