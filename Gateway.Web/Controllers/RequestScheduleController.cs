using System;
using System.Web.Mvc;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Batches.Interfaces;
using Gateway.Web.Services.Schedule.Interfaces;
using Gateway.Web.Services.Schedule.Utils;

namespace Gateway.Web.Controllers
{
    [RoutePrefix("Schedule/Request")]
    public class RequestScheduleController : Controller
    {
        private readonly IBatchConfigDataService _batchConfigDataService;
        private readonly IScheduleDataService _scheduleDataService;
        private readonly IScheduleService<ScheduleWebRequestModel> _scheduleWebRequestService;
        private readonly IScheduleGroupService _scheduleGroupService;

        public RequestScheduleController(IScheduleDataService scheduleDataService,
            IBatchConfigDataService batchConfigDataService,
            IScheduleService<ScheduleBatchModel> scheduleBatchService,
            IScheduleService<ScheduleWebRequestModel> scheduleWebRequestService,
            IScheduleGroupService scheduleGroupService)
        {
            _scheduleDataService = scheduleDataService;
            _batchConfigDataService = batchConfigDataService;
            _scheduleWebRequestService = scheduleWebRequestService;
            _scheduleGroupService = scheduleGroupService;
        }

        [HttpGet]
        [Route("Update/{id}")]
        public ActionResult UpdateRequest(long id)
        {
            var schedule = _scheduleDataService.GetSchedule(id);
            var model = schedule.ToReqeustInputModel();
            model.SetData(_batchConfigDataService);
            return View("Create", model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateOrUpdateRequest(ScheduleWebRequestModel model)
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

                    _scheduleWebRequestService.ScheduleBatches(model);
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
        [ValidateInput(false)]
        public ActionResult AssignWebRequestToGroup(ScheduleWebRequestModel model)
        {
            try
            {
                _scheduleWebRequestService.ScheduleBatches(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return RedirectToAction("Update","Schedule");
        }
    }
}