using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MSF.ClientAPI.Model;
using Bagl.Cib.MSF.Contracts.Model;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Models.Home;
using Gateway.Web.Models.Monitoring;
using Gateway.Web.Models.Request;
using Gateway.Web.Models.Security;
using Gateway.Web.Models.ServerResource;
using Gateway.Web.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using QueueChartModel = Gateway.Web.Models.Controller.QueueChartModel;

namespace Gateway.Web.Database
{
    public class GatewayDatabaseService : IGatewayDatabaseService
    {
        private readonly string _connectionString;
        private readonly IRedisService _redisService;

        public GatewayDatabaseService(ISystemInformation systemInformation, RedisService redisService)
        {
            _connectionString = systemInformation.GetConnectionString("GatewayDatabase", "Database.PnRFO_Gateway");
            _redisService = redisService;
        }

        public List<ControllerStats> GetControllerStatistics(DateTime start, string controllerName)
        {
            var result = new List<ControllerStats>();

            using (var database = new GatewayEntities(_connectionString))
            {
                // Get stats
                var stats = new ResponseStats(database.spGetResponseStatsAll(start, controllerName));
                foreach (var controller in database.Controllers.OrderBy(c => c.Name))
                {
                    var activeVersion = controller?.Versions?.FirstOrDefault(x => x.StatusId == 2 && x.Alias == "Official");

                    var redisSummary = _redisService.GetRedisSummary(controller.Name.ToLower(), activeVersion?.Version1, controller.MaxPriority);
                    var model = controller.ToModel(stats);
                    model.RedisSummary = redisSummary;
                    result.Add(model);
                }
            }

            // Insert alphabet
            var previous = ' ';
            for (int index = 0; index < result.Count; index++)
            {
                var current = Char.ToUpper(result[index].Name[0]);
                if (current != previous)
                {
                    result.Insert(index, new ControllerStats() { Name = current.ToString(), IsSeperator = true });
                    index++;
                }
                previous = current;
            }

            return result;
        }

        public List<ControllerDetail> GetControllerDetails()
        {
            var result = new List<ControllerDetail>();

            using (var database = new GatewayEntities(_connectionString))
            {
                foreach (var controller in database.Controllers.OrderBy(c => c.Name))
                {
                    var activeVersion = controller?.Versions?.FirstOrDefault(x => x.StatusId == 2 && x.Alias == "Official");
                    result.Add(new ControllerDetail()
                    {
                        MaxPriority = controller.MaxPriority,
                        DisplayName = controller.Name,
                        Name = controller.Name.ToLower(),
                        Version = activeVersion?.Version1
                    });
                }
            }

            return result;
        }

        public List<string> GetControllerNames()
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                return database.Controllers.OrderBy(c => c.Name).Select(x => x.Name).ToList();
            }
        }

        public List<HistoryItem> GetRecentRequests(DateTime start)
        {
            var result = new List<HistoryItem>();
            using (var database = new GatewayEntities(_connectionString))
            {
                var items = database.spGetRecentRequestsAll(start, "");
                foreach (var item in items)
                {
                    result.Add(item.ToModel());
                }
            }
            return result;
        }

        public IList<spGetHistoryTimingForSchedule_Result> GetHistoryTimingForSchedule(long scheduleId, int days)
        {
            using (var database = new GatewayEntities(_connectionString, 300))
            {
                var results = database.spGetHistoryTimingForSchedule(scheduleId, days);
                return results.ToList();
            }
        }

        public List<HistoryItem> GetRecentRequests(string controller, DateTime start, string search = null)
        {
            var result = new List<HistoryItem>();
            using (var database = new GatewayEntities(_connectionString))
            {
                var items = database.spGetRecentRequests(start, controller, search, null).ToList();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    items = items
                        .Where(x => x.Resource.Contains(search))
                        .ToList();
                }

                foreach (var item in items)
                {
                    result.Add(item.ToModel());
                }
            }
            return result;
        }

        public List<HistoryItem> GetRecentUserRequests(string user, DateTime start)
        {
            var result = new List<HistoryItem>();
            using (var database = new GatewayEntities(_connectionString))
            {
                var items = database.spGetRecentUserRequests(start, user);
                foreach (var item in items)
                {
                    result.Add(item.ToModel());
                }
            }
            return result;
        }

        public ResponseStats GetResponseStats(DateTime start, string contollerName)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                return new ResponseStats(database.spGetResponseStatsAll(start, contollerName));
            }
        }

        public AliasesModel GetAliases()
        {
            var result = new AliasesModel();
            using (var database = new GatewayEntities(_connectionString))
            {
                var versions = database.Versions.Where(v => v.Status.Name != "Deleted").ToArray();
                foreach (var version in versions)
                {
                    if (string.IsNullOrEmpty(version.Alias)) continue;

                    foreach (var aliasName in version.Alias.Split(','))
                    {
                        var alias = result.GetOrAdd(aliasName);
                        alias.Controllers.Add(new ControllerVersion(version.Controller.Name, version.Version1,
                            version.Status.Name, version.Status.IsActive));
                    }
                }

                var latestGroups = versions.Where(v => v.Status.Name.ToLower() == "active").GroupBy(v => v.ControllerId);
                var latest = result.GetOrAdd(" Latest");
                foreach (var group in latestGroups)
                {
                    var version = group.OrderBy(v => System.Version.Parse(v.Version1)).Last();
                    latest.Controllers.Add(new ControllerVersion(version.Controller.Name, version.Version1,
                        version.Status.Name, version.Status.IsActive));
                }
            }
            return result;
        }

        public RequestsChartModel GetControllerRequestSummary(string name, DateTime start)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var controller = database.Controllers.FirstOrDefault(c => c.Name == name);
                if (controller == null)
                    return new RequestsChartModel("Unknown controller");

                // Get recent requests
                var results = database.spGetRequestStats(start, name);
                var result = new RequestsChartModel(name);
                result.Name = name;
                foreach (var item in results.OrderBy(r => r.Date))
                {
                    if (item.Count != null && item.Date != null)
                    {
                        var label = item.Date.Value.ToString("dd MMM");
                        result.RequestSummary.Add(new ChartItem(label, item.Count.Value));
                    }
                }

                return result;
            }
        }

        public TimeChartModel GetControllerTimeSummary(string name, DateTime start)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var controller = database.Controllers.FirstOrDefault(c => c.Name == name);
                if (controller == null)
                    return new TimeChartModel("Unknown controller");

                // Get recent requests
                var results = database.spGetTimeStats(start, name);
                var result = new TimeChartModel(name);
                result.Name = name;
                foreach (var item in results.OrderBy(r => r.Date).ThenBy(r => TimeSpan.Parse(r.Hour)))
                {
                    var label = item.Date.Value.ToString("dd MMM") + " " + item.Hour;
                    var value = item.AvgTime.Value;
                    var value2 = item.MaxTime.Value;
                    result.TimeSummary.Add(new ChartItem(label, value, value2));
                }

                return result;
            }
        }

        public LinksModel GetLinks()
        {
            var result = new LinksModel();
            using (var database = new GatewayEntities(_connectionString))
            {
                var items = database.Links.ToArray().Select(l => l.ToModel());
                result.Items.AddRange(items);
            }
            return result;
        }

        public void DeleteLink(long id)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var item = database.Links.FirstOrDefault(l => l.Id == id);
                if (item != null)
                    database.Links.Remove(item);
                database.SaveChanges();
            }
        }

        public void AddLink(LinkModel link)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var target = database.Links.Create();
                target.Name = link.Name;
                target.Type = link.Type;
                target.Glyph = link.Glyph;
                target.AdditionalData = link.AdditionalData;
                target.Permission = link.Permission;
                database.Links.Add(target);
                database.SaveChanges();
            }
        }

        public GatewayRequest GetRequestClone(string correlationId)
        {
            var id = Guid.Parse(correlationId);
            using (var database = new GatewayEntities(_connectionString))
            {
                var original = database.Requests.FirstOrDefault(r => r.CorrelationId == id);
                var result = GetRequest(original);
                result.ParentCorrelationId = original.ParentCorrelationId;
                result.Query = original.Resource;
                if (!string.Equals(original.RequestType, "GET", StringComparison.CurrentCultureIgnoreCase))
                {
                    var originalPayload = database.Payloads.FirstOrDefault(r => r.CorrelationId == id && r.Direction == "Request");
                    if (originalPayload != null)
                    {
                        var comp = (CompressionType)Enum.Parse(typeof(CompressionType), originalPayload.CompressionType);
                        result.SetBody(originalPayload.Data, comp, originalPayload.PayloadType);
                    }
                }

                return result;
            }
        }

        private GatewayRequest GetRequest(Request model)
        {
            switch (model.RequestType.ToUpper())
            {
                case "GET":
                    return new Get(model.Controller, model.Version);

                case "PUT":
                    return new Put(model.Controller, model.Version);

                case "POST":
                    return new Post(model.Controller, model.Version);

                case "DELETE":
                    return new Delete(model.Controller, model.Version);

                default:
                    throw new InvalidOperationException("Unknown request type " + model.RequestType);
            }
        }

        public Summary GetRequestSummary(string correlationId)
        {
            var result = new Summary();
            var id = Guid.Parse(correlationId);
            using (var database = new GatewayEntities(_connectionString))
            {
                var request = database.Requests.FirstOrDefault(r => r.CorrelationId == id);
                var response = database.Responses.FirstOrDefault(r => r.CorrelationId == id);
                PopulateFields(result, request);
                PopulateFields(result, response);
                result.CorrelationId = id;

                var end = result.EndUtc > DateTime.MinValue ? result.EndUtc : DateTime.UtcNow;
                result.WallClockTime = (end - result.StartUtc).ToString("h'h 'm'm 's's'");

                foreach (var item in database.spGetRequestChildSummary(id).OrderBy(r => r.MinStartUtc))
                    result.Items.Add(item.ToModel());
            }
            return result;
        }

        public List<HistoryItem> GetRequestChildren(Guid correlationId)
        {
            var result = new List<HistoryItem>();
            using (var database = new GatewayEntities(_connectionString))
            {
                var items = database.spGetChildRequests(correlationId);
                foreach (var item in items)
                {
                    result.Add(item.ToModel());
                }
            }
            return result;
        }

        public Payloads GetRequestPayloads(string correlationId)
        {
            var result = new Payloads();
            var id = Guid.Parse(correlationId);
            using (var database = new GatewayEntities(_connectionString))
            {
                var request = database.Requests.FirstOrDefault(r => r.CorrelationId == id);
                result.CorrelationId = id;
                if (request != null)
                {
                    result.Controller = request.Controller;
                    result.Version = request.Version;
                }
                foreach (var item in database.spGetPayloads(id))
                {
                    result.Items.Add(new PayloadModel(item));
                }
            }

            if (result.Items.Count < 2)
            {
                if (result.Items.All(i => i.Direction != "Response"))
                {
                    result.Items.Add(new PayloadModel("Response"));
                }
                if (result.Items.All(i => i.Direction != "Request"))
                {
                    result.Items.Insert(0, new PayloadModel("Request"));
                }
            }
            return result;
        }

        public Transitions GetRequestTransitions(string correlationId)
        {
            var result = new Transitions();
            var id = Guid.Parse(correlationId);
            result.CorrelationId = id;
            using (var database = new GatewayEntities(_connectionString))
            {
                var items = database.RequestChanges.Where(r => r.CorrelationId == id).ToArray();
                foreach (var item in items)
                {
                    result.Items.Add(item.ToModel());
                }
            }
            return result;
        }

        public PayloadData GetPayload(long id)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var payload = database.Payloads.FirstOrDefault(p => p.Id == id);
                return new PayloadData(payload);
            }
        }

        public string GetBatchName(long scheduleId)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var schedule = database.Schedules.FirstOrDefault(s => s.ScheduleId == scheduleId);
                if (schedule?.RiskBatchSchedule == null) return null;

                var name = schedule.RiskBatchSchedule.RiskBatchConfiguration.Type;
                var tradeSource = schedule.RiskBatchSchedule.TradeSource;

                return $"{name} - {tradeSource}";
            }
        }

        public ReportsModel GetUsage()
        {
            var result = new ReportsModel("Usage Report");
            using (var database = new GatewayEntities(_connectionString))
            {
                var rows = database.spGetUserReport(DateTime.Today.AddDays(-7));
                var target = new UserRecentRequests();

                var table = new CubeBuilder()
                    .AddColumn("User")
                    .AddColumn("Last Request")
                    .AddColumn("Last 60 Minutes")
                    .AddColumn("Last 24 Hours")
                    .AddColumn("Last 7 Days")
                    .AddColumn("Groups")
                    .Build();
                table.SetAttribute("Title", "Recent Requests");

                foreach (var row in rows)
                {
                    target.Add(row);
                }

                foreach (var line in target.GetAll())
                {
                    var fulluser = line.User;
                    var user = fulluser;
                    if (fulluser.Contains("\\"))
                        user = user.Substring(fulluser.IndexOf("\\") + 1);
                    var link = string.Format("<a href='../../User/History?id=0&login={0}'>{1}</a>", fulluser, user);

                    table.AddRow(new object[]
                    {
                        link,
                        line.Latest.ToString("dd MMM HH:mm:ss"),
                        line.Total60Minutes.ToString(),
                        line.Total24Hours.ToString(),
                        line.Total7Days.ToString(),
                        line.Groups
                    });
                }
                result.Add(table);
            }
            return result;
        }

        private void PopulateFields(Summary result, Response response)
        {
            if (response == null) return;

            result.EndUtc = response.EndUtc;
            result.QueueTimeMs = response.QueueTimeMs;
            result.TimeTakenMs = response.TimeTakeMs;
            result.ResultCode = response.ResultCode;
            result.ResultMessage = response.ResultMessage;
            result.UpdateTime = response.UpdateTime;
        }

        private void PopulateFields(Summary result, Request request)
        {
            if (request == null) return;
            result.ParentCorrelationId = request.ParentCorrelationId;
            result.User = request.User;
            result.IpAddress = request.IpAddress;
            result.Controller = request.Controller;
            result.Version = request.Version;
            result.Resource = request.Resource;
            result.RequestType = request.RequestType;
            result.Priority = request.Priority;
            result.IsAsync = request.IsAsync;
            result.StartUtc = request.StartUtc;
            result.Client = string.IsNullOrEmpty(request.ClientID) ? "Unknown" : request.ClientID;
        }

        public IEnumerable<ControllerState> GetControllerStates(IDictionary<string, ServerDiagnostics> serverDiagnostics)
        {
            var result = new List<ControllerState>();

            try
            {
                using (var database = new GatewayEntities(_connectionString))
                {
                    var items = database.spGetControllerStates().ToList();
                    foreach (var item in items.GroupBy(i => i.Controller))
                    {
                        var controllerState = item.ToArray().ToModel();
                        SetControllerStatistics(controllerState, serverDiagnostics);
                        result.Add(controllerState);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return result;
        }

        private void SetControllerStatistics(ControllerState controllerState, IDictionary<string, ServerDiagnostics> serverDiagnostics)
        {
            if (serverDiagnostics == null)
                return;

            foreach (var serverDiagnostic in serverDiagnostics.Values)
            {
                var controllerRequestsDiagnostic = serverDiagnostic.Requests.Where(x => x.Name.ToLower() == controllerState.Name.ToLower()).ToList();
                var controllerWorkerDiagnostic = serverDiagnostic.Workers.Where(x => x.Name.ToLower() == controllerState.Name.ToLower()).ToList();

                var requestCount = controllerRequestsDiagnostic.Sum(x => x.RequestCount);
                var inProgressRequestCount = controllerRequestsDiagnostic.Sum(x => x.RequestInProgressCount);
                var workerCount = controllerWorkerDiagnostic.Sum(x => x.WorkerCount);
                var workerInProgressCount = controllerWorkerDiagnostic.Sum(x => x.WorkerInProgressCount);

                controllerState.TotalRequestCount = requestCount;
                controllerState.ProcessingRequestCount = inProgressRequestCount;
                controllerState.BusyWorkerCount = workerInProgressCount;
                controllerState.TotalWorkerCount = workerCount;
            }
        }

        public IEnumerable<string> GetVersions(string controllerName)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var controller = database.Controllers.FirstOrDefault(x => x.Name.ToLower() == controllerName.ToLower());

                if (controller == null)
                    throw new ArgumentException($"Unable to locate controller named {controllerName}");

                return database.Versions.Where(x => x.ControllerId == controller.Id).Select(x => x.Version1).ToList();
            }
        }

        public IEnumerable<string> GetActiveVersions(string controllerName)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var controller = database.Controllers.FirstOrDefault(x => x.Name.ToLower() == controllerName.ToLower());

                if (controller == null)
                    throw new ArgumentException($"Unable to locate controller named {controllerName}");

                var activeStatus = database.Status.SingleOrDefault(x => x.Name.ToLower() == "Active");

                if (activeStatus == null)
                {
                    throw new ArgumentException($"Unable to find status with name Active");
                }

                return database.Versions.Where(x => x.ControllerId == controller.Id && x.StatusId == activeStatus.Id).Select(x => x.Version1).ToList();
            }
        }

        public IEnumerable<string> GetActiveVersions()
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var activeStatus = database.Status.SingleOrDefault(x => x.Name.ToLower() == "Active");

                if (activeStatus == null)
                {
                    throw new ArgumentException($"Unable to find status with name Active");
                }

                return database.Versions.Where(x => x.StatusId == activeStatus.Id).Select(x => x.Version1).ToList();
            }
        }

        public IEnumerable<Status> GetVersionStatuses()
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                return database.Status.Where(s => s.Name != "Deleted").ToList();
            }
        }

        public bool HasStatusChanged(string controllerName, string versionName, string status, string alias)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var controller = database.Controllers
                    .Where(c => c.Name == controllerName).Select(c => c).First();

                if (controller != null)
                {
                    var version = controller.Versions
                        .Where(v => v.Version1 == versionName).Select(v => v).FirstOrDefault();

                    if (version != null)
                    {
                        return status != version.Status.Name || alias != version.Alias;
                    }
                    return status != "Inactive";
                }
                return false;
            }
        }

        public QueueChartModel GetQueueChartModel(DateTime startDate)
        {
            var model = GetHistoricalCounts(startDate);
            return model.GetSummaryForAllControllers();
        }

        public QueueChartModel GetQueueChartModel(DateTime startDate, IList<string> controllers)
        {
            var model = GetHistoricalCounts(startDate);
            return model.GetSummaryForSelectedControllers(controllers);
        }

        public async Task<List<ExtendedBatchSummary>> GetBatchSummaryStatsAsync(DateTime valuationDate)
        {
            var results = new List<ExtendedBatchSummary>();

            using (var database = new GatewayEntities(_connectionString))
            {
                var requests = GetRiskBatchRequests(database, valuationDate).ToArray();

                foreach (var item in requests)
                {
                    var resource = item.Resource;

                    var scheduleId = long.Parse(resource.Split('/')[2]);
                    var schedule = database.Schedules.SingleOrDefault(x => x.ScheduleId == scheduleId);

                    // Get all children
                    var children = GetChildRequests(database, item.CorrelationId);
                    ChildRequest[] pricingRequests, marketDataRequests, riskDataRequests, tradeStoreRequests;
                    if (!children.TryGetValue("pricing", out pricingRequests))
                        pricingRequests = new ChildRequest[0];

                    if (!children.TryGetValue("marketdata", out marketDataRequests)) //Count only
                        marketDataRequests = new ChildRequest[0];
                    if (!children.TryGetValue("riskdata", out riskDataRequests)) //Count only
                        riskDataRequests = new ChildRequest[0];
                    if (!children.TryGetValue("tradestore", out tradeStoreRequests)) // Sum Size
                        tradeStoreRequests = new ChildRequest[0];

                    var pricingResults = new Dictionary<string, Tuple<int, int>>();
                    string calculationName = string.Empty;
                    foreach (var pricingRequest in pricingRequests)
                    {
                        var calculationNameUnmapped = GetCalculationName(pricingRequest.Resource);
                        calculationName = TransformCalculationName(calculationNameUnmapped);

                        if (pricingResults.ContainsKey(calculationName))
                            continue;

                        var totalRequests = pricingRequests.Count(x => x.Resource.EndsWith(calculationNameUnmapped));
                        var successfulRequests = pricingRequests.Count(x => x.ResultCode == 1 && x.Resource.EndsWith(calculationNameUnmapped));

                        pricingResults.Add(calculationName, new Tuple<int, int>(successfulRequests, totalRequests));
                    }

                    var request = database.Requests.FirstOrDefault(r => r.CorrelationId == item.CorrelationId);
                    var response = database.Responses.FirstOrDefault(r => r.CorrelationId == item.CorrelationId);

                    var summary = ModelEx.ToExtendedBatchSummary("Unknown", request, response);
                    summary.CalculationPricingRequestResults = pricingResults;
                    if (schedule != null)
                    {
                        if (schedule.RiskBatchSchedule?.TradeSourceType == "Portfolio")
                            summary.Name = schedule.Name;
                        else
                            summary.Name = schedule.RiskBatchSchedule?.RiskBatchConfiguration.Type;
                    }
                    else
                    {
                        summary.Name = "Unknown " + calculationName;
                    }

                    summary.Trades = tradeStoreRequests.Sum(r => r.Size);
                    summary.PricingRequests = pricingRequests.Length;
                    summary.MarketDataRequests = marketDataRequests.Length;
                    summary.RiskDataRequests = riskDataRequests.Length;

                    results.Add(summary);
                }
                return results;
            }
        }

        private IEnumerable<RiskBatchRequest> GetRiskBatchRequests(GatewayEntities database, DateTime valuationDate)
        {
            // Get all batch requests that are within a window and then use the resource
            // string to narrow down the valuation date
            var start = valuationDate.AddDays(-1);
            var end = valuationDate.AddDays(2);
            var items = database.Requests
                .Where(r => r.Controller == "riskbatch" && r.StartUtc > start && r.StartUtc < end &&
                            r.Resource.StartsWith("Batch/Run/"));

            var valDateStr = valuationDate.ToString("yyyy-MM-dd");
            foreach (var item in items)
            {
                if (item.Resource.Contains(valDateStr))
                    yield return new RiskBatchRequest()
                    {
                        CorrelationId = item.CorrelationId,
                        Resource = item.Resource,
                        StartUtc = item.StartUtc
                    };
            }
        }

        private Dictionary<string, ChildRequest[]> GetChildRequests(GatewayEntities database, Guid correlationId)
        {
            return database.Requests
                .Join(database.Responses, r => r.CorrelationId, rs => rs.CorrelationId,
                    (rq, rs) => new { request = rq, response = rs })
                .Where(x => x.request.ParentCorrelationId == correlationId)
                .ToArray()
                .Select(x => new ChildRequest(x.request.CorrelationId, x.request.Controller, x.request.Resource, x.response.ResultCode, x.response.Size))
                .GroupBy(c => c.Controller)
                .ToDictionary(g => g.Key, g => g.ToArray());
        }

        public ControllerServersModel GetControllerServers(string controllerName)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var controller = database.Controllers.FirstOrDefault(x => x.Name == controllerName);

                if (controller is null) return null;

                var controllerServer = new ControllerServersModel(controller.Id, controller.Name)
                {
                    Servers = database.Servers.OrderBy(x => x.Name).Select(sel => new ControllerServer()
                    {
                        ServerId = sel.Id,
                        CPUCores = sel.CpuCores,
                        Domain = sel.Domain,
                        RAM = sel.RAM,
                        ServerName = sel.Name,
                        Allowed = sel.Controllers.Any(x => x.Id == controller.Id)
                    }).ToList()
                };

                return controllerServer;
            }
        }

        public void GetHistoryTimingsForSchedule(long scheduleId, int days)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
            }
        }

        public void UpdateControllerServers(ControllerServersModel controllerServers)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var controller = database.Controllers.FirstOrDefault(x => x.Id == controllerServers.ControllerId);
                if (controller == null)
                {
                    return;
                }

                var eligibleServers = database.Servers.ToList()
                    .Where(x => controllerServers.Servers.Any(s => s.Allowed && x.Id == s.ServerId)).ToList();

                controller.Servers.Clear();
                controller.Servers = new Collection<Server>(eligibleServers);

                database.SaveChanges();
            }
        }

        public IList<Server> GetServers()
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var servers = database.Servers.OrderBy(x => x.Name).ToList();
                return servers;
            }
        }

        public ServerControllerModel GetSeverControllers(int serverId)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var server = database.Servers.FirstOrDefault(x => x.Id == serverId);

                if (server is null) return null;

                var serverController = new ServerControllerModel()
                {
                    ServerId = server.Id,
                    ServerName = server.Name,
                    Controllers = database.Controllers.OrderBy(x => x.Name)
                    .Select(sel => new ServerController()
                    {
                        Id = sel.Id,
                        Name = sel.Name,
                        Description = sel.Description,
                        Allowed = sel.Servers.Any(x => x.Id == server.Id)
                    }).ToList()
                };

                return serverController;
            }
        }

        public void UpdateServerControllers(ServerControllerModel serverControllerModel)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var server = database.Servers.FirstOrDefault(x => x.Id == serverControllerModel.ServerId);

                if (server is null) return;

                var eligibleControllers = database.Controllers.ToList().Where(x => serverControllerModel.Controllers.Any(s => s.Allowed && x.Id == s.Id)).ToList();

                server.Controllers.Clear();
                server.Controllers = new Collection<Controller>(eligibleControllers);

                database.SaveChanges();
            }
        }

        private HistoricalSummary GetHistoricalCounts(DateTime start)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var results = database.spGetRequestCounts(start)
                                      .Select(r => r.ToModel())
                                      .ToArray();
                return new HistoricalSummary(results);
            }
        }

        private string TransformCalculationName(string name)
        {
            if (name.ToLower() == "pv")
                return "PV";

            if (name.ToLower().StartsWith("xvacalculationwith"))
                return "XVA";

            if (name.ToLower() == "generalriskreport")
                return "Risk";

            return name;
        }

        private string GetCalculationName(string resource)
        {
            if (resource == null)
                return null;

            var lastIndex = resource.LastIndexOf('-');
            return resource.Substring(lastIndex + 1).Trim();
        }

        public IEnumerable<T> GetChildMessages<T>(Guid correlationId, Func<spGetRequestChildrenPayloadDetails_Result, T> converter)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var records = database.spGetRequestChildrenPayloadDetails(correlationId);

                foreach (var record in records)
                    yield return converter(record);
            }
        }

        public ControllerDetail GetControllerDetail(string controllerName)
        {
            using (var database = new GatewayEntities(_connectionString))
            {
                var controller = database.Controllers.FirstOrDefault(c => c.Name == controllerName);
                var activeVersion = controller?.Versions?.FirstOrDefault(x => x.StatusId == 2 && x.Alias == "Official");
                return new ControllerDetail()
                {
                    MaxPriority = controller.MaxPriority,
                    DisplayName = controller.Name,
                    Name = controller.Name.ToLower(),
                    Version = activeVersion?.Version1
                };
            }
        }
    }

    internal class ChildRequest
    {
        public Guid CorrelationId { get; }
        public string Resource { get; }
        public string Controller { get; }
        public int ResultCode { get; }
        public int? Size { get; }

        public ChildRequest(Guid correlationId, string controller, string resource, int resultCode, int? size)
        {
            CorrelationId = correlationId;
            Controller = controller;
            Resource = resource;
            ResultCode = resultCode;
            Size = size;
        }
    }

    internal class RiskBatchRequest
    {
        public Guid CorrelationId { get; set; }
        public string Resource { get; set; }
        public DateTime StartUtc { get; set; }
    }

    internal class ControllerVersionSummaryQueueSize
    {
        public string Controller { get; set; }

        //public string Version { get; set; }
        public int Count { get; set; }
    }

    public class QueueSizeModel
    {
        public DateTime Time { get; set; }
        public string Label { get; set; }
        public int Count { get; set; }
    }
}