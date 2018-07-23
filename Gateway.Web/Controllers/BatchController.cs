using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Gateway.Web.Enums;
using Gateway.Web.Models.Batches;
using Gateway.Web.Services.Batches;

namespace Gateway.Web.Controllers
{
    [RoutePrefix("Batch")]
    public class BatchController : Controller
    {
        private readonly IRiskBatchConfigService _riskBatchConfigService;

        public BatchController(IRiskBatchConfigService riskBatchConfigService)
        {
            _riskBatchConfigService = riskBatchConfigService;
        }
        
        [HttpGet]
        public ActionResult Index(int page = 1, int pageSize = 10, string searchTerm = null)
        {
            var offset = (page - 1) * pageSize;
            var batchConfigList = _riskBatchConfigService.GetConfigurations(offset, pageSize, searchTerm);
            return View("Index", batchConfigList);
        }

        [HttpGet]
        public ActionResult Create(string configurationTemplate)
        {
            BatchConfigModel model = null;

            var configTypes = GetConfigurationTypes();

            if (string.IsNullOrWhiteSpace(configurationTemplate))
            {
                model = new BatchConfigModel();
                model.ConfigurationTemplates = configTypes;
                return View(model);
            }

            model = _riskBatchConfigService.GetConfiguration(configurationTemplate) ?? new BatchConfigModel();
            model.ConfigurationTemplates = configTypes;
            model.ConfigurationId = 0;

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
            var model = new DeleteBatchConfigModel
            {
                ConfigurationId = id,
                Type = type
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

        [HttpPost]
        public RedirectToRouteResult CreateOrUpdate(BatchConfigModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    TradeSourceType tradeSourceType;
                    if (!Enum.TryParse(model.TradeSourceType, out tradeSourceType))
                    {
                        ModelState.AddModelError(nameof(model.TradeSourceType), "Invalid Trade Source Type provided.");
                    }

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
                var routeValueDictionary = new RouteValueDictionary { { "configurationTemplate", model.Type } };
                return RedirectToAction("Create", routeValueDictionary);
            }

            if (model.CreateAnother)
            {
                return RedirectToAction("Create");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ReturnToCreateWithErrors(BatchConfigModel model)
        {
            model.ConfigurationTemplates = GetConfigurationTypes();
            return View("Create", model);
        }

        private IList<SelectListItem> GetConfigurationTypes()
        {
            var types = _riskBatchConfigService.GetConfigurationTypes();
            types.Insert(0, null);

            return types
                .Select(x => new SelectListItem { Value = x, Text = x })
                .ToList();
        }
    }
}