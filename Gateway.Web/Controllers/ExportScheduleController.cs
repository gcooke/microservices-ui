using AutoMapper;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Enums;
using Gateway.Web.Models.Export;
using Gateway.Web.Services;
using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Serialization;

namespace Gateway.Web.Controllers
{
    [RoutePrefix("ExportSchedule")]
    public class ExportScheduleController : BaseController
    {
        private readonly IExportService _exportService;

        public ExportScheduleController(ILoggingService loggingService, IExportService exportService) : base(
            loggingService)
        {
            _exportService = exportService;
        }

        [HttpGet]
        [Route("")]
        public ActionResult Index(DateTime? businessDate = null)
        {
            var model = new ExportSchedularViewModel();
            if (!businessDate.HasValue)
                model.BusinessDate = DateTime.Now;
            else
                model.BusinessDate = businessDate.Value;

            var groups = _exportService.FetchExports(model.BusinessDate);
            if (groups.Count > 0)
                model.ExportCrons = groups;
            return View(model);
        }

        [HttpGet]
        [Route("Update")]
        public ActionResult Update(long? id = null, bool copy = false)
        {
            var model = new ExportUpdateViewModel()
            {
                StartDateTime = DateTime.Now.Date
            };

            if (id.HasValue && id.Value > 0)
            {
                var fileExport = _exportService.FetchExport(id.Value);
                model = Mapper.Map<ExportUpdateViewModel>(fileExport);

                if (copy)
                {
                    model.ExportId = 0;
                    model.Name = "Copy of " + model.Name;
                }

                CubeToCsvSourceInformation sourceInfoResult = null;
                CubeToCsvDestinationInformation destinationInfoResult = null;
                var serializer = new XmlSerializer(typeof(CubeToCsvSourceInformation));
                using (var reader = new StringReader(fileExport.SourceInformation))
                {
                    sourceInfoResult = (CubeToCsvSourceInformation)serializer.Deserialize(reader);
                }

                serializer = new XmlSerializer(typeof(CubeToCsvDestinationInformation));
                using (var reader = new StringReader(fileExport.DestinationInformation))
                {
                    destinationInfoResult = (CubeToCsvDestinationInformation)serializer.Deserialize(reader);
                }

                model.SourceInformation = Mapper.Map<SourceInformationViewModel>(sourceInfoResult);
                model.DestinationInformation = Mapper.Map<DestinationInfoViewModel>(destinationInfoResult);
            }

            return View(model);
        }

        [HttpGet]
        [Route("Rerun/{id}/{businessDate}")]
        public void RerunSchedule(long id, DateTime businessDate)
        {
            _exportService.RunExport(id, businessDate);
        }

        [HttpPost]
        [Route("CreateOrUpdate")]
        public ActionResult CreateOrUpdate(ExportUpdateViewModel model)
        {
            if (model.ExportId == 0)
            {
                var insert = _exportService.CreateExport(ConvertViewModel(model));
                if (insert.Id.HasValue)
                    model.ExportId = insert.Id.Value;
            }
            else
                _exportService.UpdateExport(ConvertViewModel(model));

            return RedirectToAction("Index");
        }

        private ExportSchedule ConvertViewModel(ExportUpdateViewModel model)
        {
            var exportUpdate = new ExportSchedule()
            {
                DestinationInfo = Mapper.Map<CubeToCsvDestinationInformation>(model.DestinationInformation),
                Schedule = model.Schedule,
                Name = model.Name,
                IsDisabled = model.IsDisabled,
                Type = (ExportType)Enum.Parse(typeof(ExportType), model.Type),
                StartDateTime = model.StartDateTime,
                IsDeleted = model.IsDeleted,
                SourceInfo = Mapper.Map<CubeToCsvSourceInformation>(model.SourceInformation),
                FailureEmailAddress = model.FailureEmailAddress,
                SuccessEmailAddress = model.SuccessEmailAddress
            };

            if (model.ExportId > 0)
                exportUpdate.Id = model.ExportId;

            return exportUpdate;
        }
    }
}