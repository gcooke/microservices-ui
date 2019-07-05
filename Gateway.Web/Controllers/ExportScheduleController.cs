using Bagl.Cib.MIT.Logging;
using Gateway.Web.Models.Export;
using Gateway.Web.Services;
using System;
using System.Web.Mvc;
using System.Web.Routing;

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
        public ActionResult Update(long? id = null)
        {
            var model = new ExportUpdateViewModel()
            {
                StartDateTime = DateTime.Now.Date,
                IsDisabled = true,
                IsDeleted = true
            };
            return View(model);
        }

        [HttpPost]
        [Route("CreateOrUpdate")]
        public ActionResult CreateOrUpdate(ExportUpdateViewModel model)
        {
            if (model.ExportId > 0)
            {
                var insert = _exportService.CreateExport(ConvertViewModel(model));
                if (insert.Id.HasValue)
                    model.ExportId = insert.Id.Value;
            }
            else
                _exportService.UpdateExport(ConvertViewModel(model));

            return View(model);
        }

        private ExportUpdate ConvertViewModel(ExportUpdateViewModel model)
        {
            throw new NotImplementedException();
        }
    }
}