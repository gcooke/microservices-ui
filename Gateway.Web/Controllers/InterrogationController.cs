using System;
using System.Globalization;
using System.Text;
using System.Web.Mvc;
using Bagl.Cib.MIT.Cube.Utils;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;
using Gateway.Web.Models.Interrogation;
using Gateway.Web.Services;
using Gateway.Web.Services.Batches.Interrogation.Models.Enums;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class InterrogationController : BaseController
    {
        private readonly IRiskBatchInterrogationService _service;

        public InterrogationController(IRiskBatchInterrogationService service, ILoggingService loggingService)
            : base(loggingService)
        {
            _service = service;
        }

        public ActionResult Index(string tradeSource, string batchType, string reportDateString, string minimumLevelInput)
        {
            var model = new InterrogationModel();
            _service.PopulateLookups(model);
            model.TradeSource = tradeSource ?? model.TradeSource;
            model.BatchType = batchType ?? model.BatchType;
            model.ReportDateString = reportDateString ?? model.ReportDateString;
            model.MinimumLevelInput = minimumLevelInput ?? model.MinimumLevelInput;
            _service.Analyze(model);
            return View("RiskBatch", model);
        }

        public ActionResult Details(string tradeSource, string batchType, string reportDateString, string minimumLevelInput)
        {
            var model = new InterrogationModel();
            model.TradeSource = tradeSource;
            model.BatchType = batchType;
            model.ReportDate = DateTime.ParseExact(reportDateString, "yyyy-MM-dd", CultureInfo.CurrentUICulture);
            model.MinimumLevel = (MonitoringLevel)Enum.Parse(typeof(MonitoringLevel), minimumLevelInput);
            _service.PopulateLookups(model);
            _service.Analyze(model);

            return View("RiskBatch", model);
        }

        public ActionResult XvaReport(string correlationId, string reportDateString, string allRows)
        {
            var model = _service.GetXvaReport(correlationId, reportDateString, allRows);
            return View("XvaReport", model);
        }

        public FileResult DownloadXvaReport(string correlationId)
        {
            var model = _service.GetXvaReport(correlationId, string.Empty, "true");
            var cube = model.BatchStatistics.Cube;
            cube.Attributes.ClearAttributes();
            var csv = cube.ToCsv();
            var encoded = Encoding.UTF8.GetBytes(csv);

            return File(encoded, "text/csv", $"XvaReport_{model.Date.Replace(" ", "_")}.csv");
        }
    }
}