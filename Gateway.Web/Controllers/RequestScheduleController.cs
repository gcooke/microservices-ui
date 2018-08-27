using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Absa.Cib.MIT.TaskScheduling.Models;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Batches.Interfaces;
using Gateway.Web.Services.Schedule.Interfaces;
using Gateway.Web.Services.Schedule.Utils;
using Newtonsoft.Json;

namespace Gateway.Web.Controllers
{
    [RoutePrefix("Schedule/Request")]
    public class RequestScheduleController : Controller
    {
        private readonly IBatchConfigDataService _batchConfigDataService;
        private readonly IScheduleDataService _scheduleDataService;
        private readonly IScheduleService<ScheduleWebRequestModel> _scheduleWebRequestService;
        private readonly IRequestConfigurationService _requestConfigurationService;
        private readonly IScheduleGroupService _scheduleGroupService;

        public RequestScheduleController(IScheduleDataService scheduleDataService,
            IBatchConfigDataService batchConfigDataService,
            IScheduleService<ScheduleBatchModel> scheduleBatchService,
            IScheduleService<ScheduleWebRequestModel> scheduleWebRequestService,
            IRequestConfigurationService requestConfigurationService,
            IScheduleGroupService scheduleGroupService)
        {
            _scheduleDataService = scheduleDataService;
            _batchConfigDataService = batchConfigDataService;
            _scheduleWebRequestService = scheduleWebRequestService;
            _requestConfigurationService = requestConfigurationService;
            _scheduleGroupService = scheduleGroupService;
        }


        [HttpGet]
        [Route("Create/Group/{id}")]
        public ActionResult CreateForGroup(long id)
        {
            var model = new ScheduleWebRequestModel {Group = id.ToString()};
            model.SetData(_batchConfigDataService, _requestConfigurationService);
            return View("Create", model);
        }

        [HttpGet]
        [Route("Create/Group/Template")]
        public ActionResult CreateUsingTemplate(long group, long requestTemplate)
        {
            var model = new ScheduleWebRequestModel { Group = group.ToString() };
            var request = _requestConfigurationService.GetRequestConfiguration(requestTemplate);
            model.SetData(_batchConfigDataService, _requestConfigurationService);
            model.ScheduleId = 0;
            model.Group = group.ToString();
            model.Name = null;
            model.Url = request.Url;
            model.Payload = request.Payload;
            model.Verb = request.Verb;
            model.Arguments = JsonConvert.DeserializeObject<List<Argument>>(request.Arguments);
            model.Headers = JsonConvert.DeserializeObject<List<Header>>(request.Headers);
            return View("Create",model);
        }

        [HttpGet]
        [Route("Update/{id}")]
        public ActionResult UpdateRequest(long id)
        {
            var schedule = _scheduleDataService.GetSchedule(id);
            var model = schedule.ToReqeustInputModel();
            model.SetData(_batchConfigDataService, _requestConfigurationService);
            return View("Create", model);
        }

        [HttpPost]
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

                   var errorCollection = _scheduleWebRequestService.ScheduleBatches(model);
                    foreach (var errorList in errorCollection)
                    {
                        foreach (var error in errorList)
                        {
                            ModelState.AddModelError(Guid.NewGuid().ToString(), error.ErrorMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            if (!ModelState.IsValid)
            {
                model.SetData(_batchConfigDataService, _requestConfigurationService);
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