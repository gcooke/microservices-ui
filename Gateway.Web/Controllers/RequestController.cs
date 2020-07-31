using System.Text;
using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Model;
using Bagl.Cib.MSF.Contracts.Model;
using Gateway.Web.Authorization;
using Gateway.Web.Database;
using Gateway.Web.Enums;
using Gateway.Web.Helpers;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Request;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;

using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

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

        public ActionResult ViewPayLoad(string correlationId, long payloadId, string controllername)
        {
            var data = _dataService.GetPayload(payloadId);

            if (controllername.ToLower() == SigmaController.CounterpartyXVA.ToString().ToLower() && data.Direction == "Response")
            {
                var bytes = data.GetBytes();
                var str = Encoding.UTF8.GetString(bytes);
                var xvamodel = new XvaResultModel(str, correlationId, payloadId);

                var report = xvamodel.Reports.FirstOrDefault(x => x.Key.Contains("XVA Counterparty Validation"));

                var cubeBytes = Convert.FromBase64String(report.Value);
                var cube = CubeBuilder.FromBytes(cubeBytes);
                return View("Cube", new CubeModel(cube, "XVA Counterparty Validation"));
            }
            var model = new PayloadModel(new spGetPayloads_Result()
            {
                PayloadType = data.PayloadType,
                CompressionType = data.CompressionType,
                DataLengthBytes = data.DataLengthBytes,
                Data = data.Data,
                Direction = data.Direction
            });

            return View("ViewPayLoad", model);
        }

        public ActionResult ViewXvaResult(string correlationId, long payloadId)
        {
            var data = _dataService.GetPayload(payloadId);
            var bytes = data.GetBytes();
            var str = Encoding.UTF8.GetString(bytes);
            var model = new XvaResultModel(str, correlationId, payloadId);
            return View("XvaResult", model);
        }

        public ActionResult ViewXvaCubeItem(string correlationId, long payloadId, string cubeName)
        {
            var data = _dataService.GetPayload(payloadId);
            var bytes = data.GetBytes();
            var str = Encoding.UTF8.GetString(bytes);
            var model = new XvaResultModel(str, correlationId, payloadId);

            if (cubeName.Equals("Batch Statistics", StringComparison.InvariantCultureIgnoreCase))
            {
                var statsCubeBytes = Convert.FromBase64String(model.BatchStatisticsRawData);
                var statsCube = CubeBuilder.FromBytes(statsCubeBytes);
                return View("Cube", new CubeModel(statsCube, cubeName));
            }

            var report = model.Reports.SingleOrDefault(x => x.Key.Equals(cubeName, StringComparison.InvariantCultureIgnoreCase));
            if (report.Key == null)
                throw new Exception($"Data does not exist for {cubeName} for payload {payloadId} for request {correlationId}");

            var cubeBytes = Convert.FromBase64String(report.Value);
            var cube = CubeBuilder.FromBytes(cubeBytes);
            return View("Cube", new CubeModel(cube, cubeName));
        }

        public ActionResult DownloadXvaCubeItem(string correlationId, long payloadId, string cubeName)
        {
            var data = _dataService.GetPayload(payloadId);
            var bytes = data.GetBytes();
            var str = Encoding.UTF8.GetString(bytes);
            var model = new XvaResultModel(str, correlationId, payloadId);

            if (cubeName.Equals("Batch Statistics", StringComparison.InvariantCultureIgnoreCase))
            {
                var statsCubeBytes = Convert.FromBase64String(model.BatchStatisticsRawData);
                var statsCube = CubeBuilder.FromBytes(statsCubeBytes);
                return File(statsCube.ToBytes(), "application/octet-stream", $"Cube_{cubeName}.dat");
            }

            var report = model.Reports.SingleOrDefault(x => x.Key.Equals(cubeName, StringComparison.InvariantCultureIgnoreCase));
            if (report.Key == null)
                throw new Exception($"Data does not exist for {cubeName} for payload {payloadId} for request {correlationId}");

            var cubeBytes = Convert.FromBase64String(report.Value);
            var cube = CubeBuilder.FromBytes(cubeBytes);
            return File(cube.ToBytes(), "application/octet-stream", $"Cube_{cubeName}.dat");
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

        public ActionResult BatchCompare(string id, string oldId)
        {
            var model = new BatchCompare()
            {
                CurrentSummary = new Summary(),
                OldSummary = new Summary()
            };

            if (!string.IsNullOrEmpty(id))
                model.CurrentSummary = _dataService.GetRequestSummary(id);
            if (!string.IsNullOrEmpty(oldId))
                model.OldSummary = _dataService.GetRequestSummary(oldId);
            return View(model);
        }

        public ActionResult PayloadErrors(string id)
        {
            var summary = _dataService.GetRequestSummary(id, true);

            var model = new PayloadErrors() { ErrorRows = new List<ErrorRow>() };
            if (summary != null && summary.ErrorRows.Count > 0)
            {
                model.ErrorRows = summary.ErrorRows;
            }

            Guid correlationId;

            if (Guid.TryParse(id, out correlationId))
                model.CorrelationId = correlationId;

            return View(model);
        }

        public ActionResult DeepDive(string id, string controllername)
        {
            Guid correlationId;

            var model = new DeepDive()
            {
                Controller = controllername,
                DeepDiveResults = new List<DeepDiveDto>(),
                DeepDiveSearch = new DeepDiveSearch()
                {
                    CorrelationId = id,
                    OnlyShowErrors = true
                }
            };

            if (Guid.TryParse(id, out correlationId))
                model.CorrelationId = correlationId;

            GetDeepDiveResults(model);

            return View(model);
        }

        [HttpPost]
        public ActionResult DeepDive()
        {
            Guid correlationId;
            var model = new DeepDive();

            model.DeepDiveSearch = GetDeepDiveSearch(Request);

            if (Guid.TryParse(model.DeepDiveSearch?.CorrelationId, out correlationId))
                model.CorrelationId = correlationId;

            GetDeepDiveResults(model);
            ApplySorting(model);

            return View(model);
        }

        private static void ApplySorting(DeepDive model)
        {
            var sortName = model.DeepDiveSearch.SortOrder.Replace("_Desc", "");
            var isDescending = model.DeepDiveSearch.SortOrder.Contains("_Desc");

            switch (sortName)
            {
                case "Controller":
                    if (isDescending)
                        model.DeepDiveResults = model.DeepDiveResults.OrderByDescending(x => x.Controller).ToList();
                    else
                        model.DeepDiveResults = model.DeepDiveResults.OrderBy(x => x.Controller).ToList();
                    break;

                case "Resource":
                    if (isDescending)
                        model.DeepDiveResults = model.DeepDiveResults.OrderByDescending(x => x.Resource).ToList();
                    else
                        model.DeepDiveResults = model.DeepDiveResults.OrderBy(x => x.Resource).ToList();
                    break;

                case "TimeTakenInMs":
                    if (isDescending)
                        model.DeepDiveResults = model.DeepDiveResults.OrderByDescending(x => x.TimeTakenInMs).ToList();
                    else
                        model.DeepDiveResults = model.DeepDiveResults.OrderBy(x => x.TimeTakenInMs).ToList();
                    break;

                default:
                    break;
            }
        }

        private void GetDeepDiveResults(DeepDive model)
        {
            var result = _dataService.GetDeepDive(model.DeepDiveSearch);

            if (model.DeepDiveSearch.SearchPayload)
            {
                var updatedResults = new List<DeepDiveDto>();
                foreach (var item in result)
                {
                    if (item.PayloadId.HasValue && item.PayloadId > 0 && !string.IsNullOrEmpty(model?.DeepDiveSearch?.Search))
                    {
                        var payload = _dataService.GetPayload(item.PayloadId.Value);
                        if (payload == null)
                            continue;

                        var payloadTypeValue = (PayloadType)Enum.Parse((typeof(PayloadType)), payload.PayloadType);
                        switch (payloadTypeValue)
                        {
                            case PayloadType.JObject:
                            case PayloadType.Unknown:
                            case PayloadType.String:
                            case PayloadType.XElement:

                                var payloadModel = new PayloadModel(new spGetPayloads_Result()
                                {
                                    PayloadType = payload.PayloadType,
                                    CompressionType = payload.CompressionType,
                                    DataLengthBytes = payload.DataLengthBytes,
                                    Data = payload.Data,
                                    Direction = payload.Direction
                                });

                                if (payloadModel.Data.Contains(model.DeepDiveSearch.Search))
                                    updatedResults.Add(item);
                                break;

                            case PayloadType.Binary:
                                break;

                            case PayloadType.Cube:
                                var cubeModel = new CubeModel(payload);
                                var rows = cubeModel.Rows;
                                if (rows.Contains(model.DeepDiveSearch.Search))
                                    updatedResults.Add(item);
                                break;

                            case PayloadType.MultiPart:
                                break;

                            default:
                                updatedResults.Add(item);
                                break;
                        }
                    }
                }
                model.DeepDiveResults = updatedResults;
            }
            else
            {
                model.DeepDiveResults = result.ToList();
            }
        }

        private DeepDiveSearch GetDeepDiveSearch(HttpRequestBase request)
        {
            var controller = request.Form["DeepDiveSearch.Controller"];
            var keyword = request.Form["DeepDiveSearch.Search"];
            var id = request.Form["DeepDiveSearch.CorrelationId"];
            var sortDirection = request.Form["SortDirection"];
            var searchResource = (request.Form["DeepDiveSearch.SearchResource"] == null || request.Form["DeepDiveSearch.SearchResource"] == "false") ? false : true;
            var searchMessage = (request.Form["DeepDiveSearch.SearchResultMessage"] == null || request.Form["DeepDiveSearch.SearchResultMessage"] == "false") ? false : true;
            var searchPayload = (request.Form["DeepDiveSearch.SearchPayload"] == null || request.Form["DeepDiveSearch.SearchPayload"] == "false") ? false : true;
            var onlyShowErrors = (request.Form["DeepDiveSearch.OnlyShowErrors"] == null || request.Form["DeepDiveSearch.OnlyShowErrors"] == "false") ? false : true;
            var runningChildren = (request.Form["DeepDiveSearch.RunningChildren"] == null || request.Form["DeepDiveSearch.RunningChildren"] == "false") ? false : true;

            if (sortDirection.Contains("_Desc"))
                ViewBag.SortDirection = "";
            else
                ViewBag.SortDirection = "_Desc";

            var model = new DeepDiveSearch()
            {
                CorrelationId = id,
                SearchResultMessage = searchMessage,
                SearchPayload = searchPayload,
                SearchResource = searchResource,
                OnlyShowErrors = onlyShowErrors,
                SortOrder = sortDirection,
                RunningChildren = runningChildren
            };

            if (!string.IsNullOrEmpty(keyword))
                model.Search = keyword;

            if (!string.IsNullOrEmpty(controller) && controller != "All")
                model.Controller = controller;

            return model;
        }
    }
}