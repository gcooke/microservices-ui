using Bagl.Cib.MIT.Logging;
using Gateway.Web.Enums;
using Gateway.Web.Models.Batches;
using Gateway.Web.Services.Batches.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace Gateway.Web.Controllers
{
    [RoutePrefix("Batch")]
    public class BatchController : BaseController
    {
        private readonly IBatchConfigService _riskBatchConfigService;
        private readonly IBatchParametersService _batchParametersService;

        public BatchController(IBatchConfigService riskBatchConfigService,
            IBatchParametersService batchParametersService,
            ILoggingService loggingService)
           : base(loggingService)
        {
            _riskBatchConfigService = riskBatchConfigService;
            _batchParametersService = batchParametersService;
        }

        [HttpGet]
        public ActionResult Index(int page = 1, int pageSize = 100, string searchTerm = null)
        {
            var offset = (page - 1) * pageSize;
            var batchConfigList = _riskBatchConfigService.GetConfigurations(offset, pageSize, searchTerm);
            return View("Index", batchConfigList);
        }

        [HttpGet]
        public ActionResult Create(string configurationTemplate, bool createAnother = false, bool basedOfCurrentItem = false)
        {
            BatchConfigModel model = null;

            var configTypes = GetConfigurationTypes();

            if (string.IsNullOrWhiteSpace(configurationTemplate))
            {
                model = new BatchConfigModel();
                model.ConfigurationTemplates = configTypes;
                model.CreateAnother = createAnother;
                model.CreateAnotherBasedOnCurrentConfiguration = basedOfCurrentItem;
                return View(model);
            }

            model = _riskBatchConfigService.GetConfiguration(configurationTemplate) ?? new BatchConfigModel();
            model.ConfigurationTemplates = configTypes;
            model.ConfigurationId = 0;
            model.CreateAnother = createAnother;
            model.CreateAnotherBasedOnCurrentConfiguration = basedOfCurrentItem;

            return View(model);
        }

        [HttpGet]
        [Route("Update/{id}")]
        public ActionResult Update(long id)
        {
            var model = _riskBatchConfigService.GetConfiguration(id);
            model.ConfigurationTemplates = GetConfigurationTypes();
            return View("Create", model);
        }

        [HttpGet]
        [Route("Delete/{id}/{type}")]
        public ActionResult Delete(long id, string type)
        {
            var config = _riskBatchConfigService.GetConfiguration(id);
            var model = new BatchDeleteConfigModel
            {
                ConfigurationId = id,
                Type = type.Replace("!", "."), //due to MVC special character limitation we are switching . -> ! and Vice Versa as a workaround
                ScheduleCount = config.ScheduleCount
            };
            return View("Delete", model);
        }

        [HttpGet]
        [Route("Delete/{id}")]
        public RedirectToRouteResult Delete(long id)
        {
            _riskBatchConfigService.DeleteConfiguration(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("Delete/Bulk")]
        public RedirectToRouteResult Delete(string items)
        {
            var itemList = items.Split(',').Select(long.Parse);

            foreach (var id in itemList)
            {
                _riskBatchConfigService.DeleteConfiguration(id);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public RedirectToRouteResult CreateOrUpdate(BatchConfigModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    OutputType outputType;
                    if (!Enum.TryParse(model.OutputType, out outputType))
                    {
                        ModelState.AddModelError(nameof(model.OutputType), "Invalid Output Type provided.");
                    }

                    if (model.IsUpdating)
                    {
                        _riskBatchConfigService.UpdateConfiguration(model);
                    }
                    else
                    {
                        _riskBatchConfigService.CreateConfiguration(model);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("ReturnToCreateWithErrors", model);
            }

            if (model.CreateAnother && model.CreateAnotherBasedOnCurrentConfiguration)
            {
                var routeValueDictionary = new RouteValueDictionary
                {
                    { "configurationTemplate", model.Type },
                    { "createAnother", model.CreateAnother },
                    { "basedOfCurrentItem", model.CreateAnotherBasedOnCurrentConfiguration }
                };
                return RedirectToAction("Create", routeValueDictionary);
            }

            if (model.CreateAnother)
            {
                var routeValueDictionary = new RouteValueDictionary
                {
                    { "createAnother", model.CreateAnother },
                    { "basedOfCurrentItem", model.CreateAnotherBasedOnCurrentConfiguration }
                };
                return RedirectToAction("Create", routeValueDictionary);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ReturnToCreateWithErrors(BatchConfigModel model)
        {
            model.ConfigurationTemplates = GetConfigurationTypes();
            return View("Create", model);
        }

        public async Task<ActionResult> SettingsReport()
        {
            var report = _riskBatchConfigService.GetSettingsReport();
            report = await _batchParametersService.PopulateDifferences(report);
            return View(report);
        }

        private IList<SelectListItem> GetConfigurationTypes()
        {
            var types = _riskBatchConfigService.GetConfigurationTypes();
            types.Insert(0, null);

            return types
                .Select(x => new SelectListItem { Value = x?.Type, Text = x == null ? String.Empty : $"{x?.Type} (OutputTag :{x?.OutputTag})" })
                .ToList();
        }
    }
}