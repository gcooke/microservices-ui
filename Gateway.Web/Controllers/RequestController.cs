using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Linq;
using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Model;
using Bagl.Cib.MSF.Contracts.Model;
using Gateway.Web.Authorization;
using Gateway.Web.Database;
using Gateway.Web.Helpers;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Request;
using Gateway.Web.Services;
using Gateway.Web.Utils;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class RequestController : BaseController
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;
        private readonly ILogsService _logsService;
        private readonly IBatchNameService _batchNameService;
        private readonly IUsernameService _usernameService;
        private readonly IStatisticsService _statisticsService;

        public RequestController(IGatewayDatabaseService dataService,
            IGatewayService gateway,
            ILogsService logsService,
            ILoggingService loggingService,
            IBatchNameService batchNameService,
            IUsernameService usernameService,
            IStatisticsService statisticsService)
            : base(loggingService)
        {
            _dataService = dataService;
            _gateway = gateway;
            _logsService = logsService;
            _batchNameService = batchNameService;
            _usernameService = usernameService;
            _statisticsService = statisticsService;
        }

        public ActionResult Summary(string correlationId)
        {
            var model = _dataService.GetRequestSummary(correlationId);
            if (string.Equals(model.Controller, "RiskBatch", StringComparison.CurrentCultureIgnoreCase))
                if (model.Resource.StartsWith("Batch/Run/"))
                    model.AdditionalInfo = _batchNameService.GetName(model.Resource);

            return View(model);
        }

        public ActionResult Payloads(string correlationId)
        {
            var model = _dataService.GetRequestPayloads(correlationId);
            return View(model);
        }

        public ActionResult Transitions(string correlationId)
        {
            var model = _dataService.GetRequestTransitions(correlationId);
            return View(model);
        }

        public ActionResult Logs(string correlationId)
        {
            var model = _logsService.GetLogs(correlationId);
            return View(model);
        }

        public ActionResult Children(string correlationId, string sortOrder, string filter = "")
        {
            if (string.IsNullOrEmpty(sortOrder))
                sortOrder = "time_desc";

            ViewBag.SortColumn = sortOrder;
            ViewBag.SortDirection = sortOrder.EndsWith("_desc") ? "" : "_desc";
            ViewBag.Controller = "Request";
            ViewBag.Action = "Children";

            var model = new Children();
            var id = string.IsNullOrEmpty(correlationId) ? (Guid)Session["LastCorrelationId"] : Guid.Parse(correlationId);
            Session["LastCorrelationId"] = id;
            model.CorrelationId = id;

            IEnumerable<HistoryItem> items = _dataService.GetRequestChildren(id);
            if (!string.IsNullOrEmpty(filter))
            {
                // Only include requests that match the controller name.
                items = items.Where(r => string.Equals(r.Controller, filter, StringComparison.CurrentCultureIgnoreCase));
            }

            model.Requests.AddRange(items, sortOrder);
            model.Requests.EnrichHistoryResults(_batchNameService, _usernameService);
            return View(model);
        }

        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public async Task<ActionResult> Cancel(string correlationId)
        {
            await _gateway.CancelWorkItemAsync(correlationId);
            return Redirect("~/Request/Summary?correlationId=" + correlationId);
        }

        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public async Task<ActionResult> Retry(string correlationId)
        {
            await _gateway.RetryWorkItemAsync(correlationId);
            return Redirect("~/Request/Summary?correlationId=" + correlationId);
        }

        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public async Task<ActionResult> Rerun(string correlationId)
        {
            var request = _dataService.GetRequestClone(correlationId);
            request.AddHeader("ClientID", "Redstone Dashboard");

            // This is intentionally done in another thread.
            ThreadPool.QueueUserWorkItem(new WaitCallback(RerunRequest), request);

            // Wait for gateway to start and audit request so the user sees it in the next page loaded.
            Thread.Sleep(500);

            return Redirect($"~/Controller/History/{request.Controller}");
        }

        private void RerunRequest(object state)
        {
            try
            {
                var request = (GatewayRequest)state;

                _logger.InfoFormat("Sending same request back through gateways");
                var task = _gateway.Send<string>(request);
                var result = task.Result;
                _logger.InfoFormat("Completed rerun of request. Result {0}", result.Successfull ? "was successful" : "failed: " + result.Message);
            }
            catch (Exception ex)
            {
                _logger.WarnFormat("Failed to rerun request: {0}", ex.Message);
            }
        }

        public ActionResult Download(string correlationId, long payloadId)
        {
            var data = _dataService.GetPayload(payloadId);
            var bytes = data.GetBytes();
            var extension = data.GetExtension();
            if (extension.ToLower() == "xml")
            {
                var str = Encoding.UTF8.GetString(bytes);
                var element = XElement.Parse(str);
                element = XmlSort.TrySortPayload(_logger, element);

                str = element.ToString(SaveOptions.None);
                bytes = Encoding.UTF8.GetBytes(str);
            }
            return File(bytes, "text/xml", string.Format("Payload_{0}.{1}", payloadId, extension));
        }

        public ActionResult DownloadCube(string correlationId, long payloadId)
        {
            var data = _dataService.GetPayload(payloadId);
            return File(data.GetCubeBytes(), "application/octet-stream", string.Format("Cube_{0}.dat", payloadId));
        }

        public ActionResult ViewCube(string correlationId, long payloadId)
        {
            var data = _dataService.GetPayload(payloadId);
            var model = new CubeModel(data);
            return View("Cube", model);
        }

        public ActionResult ViewXvaResult(string correlationId, long payloadId)
        {
            var data = _dataService.GetPayload(payloadId);
            var bytes = data.GetBytes();
            var str = Encoding.UTF8.GetString(bytes);
            var model = new XvaResultModel(str);
            return View("XvaResult", model);
        }

        public ActionResult ViewCubeItem(string name, string data)
        {
            var bytes = Convert.FromBase64String(data);
            var cube = CubeBuilder.FromBytes(bytes);
            var model = new CubeModel(cube, name);
            return View("Cube", model);
        }

        public ActionResult DownloadCubeItem(string name, string data)
        {
            var bytes = Convert.FromBase64String(data);
            var cube = CubeBuilder.FromBytes(bytes);
            return File(cube.ToBytes(), "application/octet-stream", $"Cube_{name}.dat");
        }

        public ActionResult ExtractCube(string correlationId, long payloadId)
        {
            var data = _dataService.GetPayload(payloadId);
            var interim = new PayloadModel("na");
            interim.SetData(data.Data, data.DataLengthBytes, data.CompressionType, data.PayloadType);
            var element = XElement.Parse(interim.Data);
            string value;
            if (element.Descendants().Any(e => e.Name.LocalName.Equals("CalcResultCube", StringComparison.InvariantCultureIgnoreCase)))
            {
                var result = element.Descendants().First(e => e.Name.LocalName.Equals("CalcResultCube", StringComparison.InvariantCultureIgnoreCase));
                value = result.Attribute("data")?.Value;
            }
            else
            {
                var result = element.Descendants().FirstOrDefault(d => d.Attribute("Property")?.Value == "cube");
                value = result?.Attribute("data")?.Value;
            }

            var cube = CubeBuilder.FromBytes(Convert.FromBase64String(value ?? string.Empty));
            var model = new CubeModel(cube);
            return View("Cube", model);
        }

        public ActionResult Timings(string id)
        {
            Guid correlationId;

            if (!Guid.TryParse(id, out correlationId))
                return View();

            var model = _statisticsService.GetTimings(correlationId);
            return View(model);
        }
    }
}