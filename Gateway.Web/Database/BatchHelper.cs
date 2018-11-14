using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models.Home;
using Gateway.Web.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bagl.Cib.MIT.Redis.Caching;

namespace Gateway.Web.Database
{
    public class BatchHelper
    {
        private string[] AllCountries = {
            "Mauritius_Onshore",
            "Mauritius_IBD",
            "South_Africa",
            "Tanzania_NBC",
            "Seychelles",
            "Mozambique",
            "Botswana",
            "Tanzania",
            "Uganda",
            "Zambia",
            "Ghana",
            "Kenya"
        };

        private readonly IGatewayDatabaseService _database;
        private readonly IGatewayRestService _gateway;
        private readonly IRedisCache _cache;

        public BatchHelper(IGatewayDatabaseService database, IGatewayRestService gateway, IRedisCache cache)
        {
            _database = database;
            _gateway = gateway;
            _cache = cache;
        }

        public BatchDetail GetBatchDetails(string name, DateTime reportDate, Guid correlationId)
        {
            var response = _gateway.Get("managementinterface",
                $"batch/{name}/{correlationId}/detail/{reportDate:yyyy-MM-dd}", CancellationToken.None);

            if (!response.Successfull || response.Content?.Payload == null)
                throw new Exception("Unable to retrieve batch error details.");

            var payload = response.Content.GetPayloadAsString();
            var data = JsonConvert.DeserializeObject<BatchDetail>(payload);

            return data;
        }

        public async Task<RiskBatchModel> GetRiskBatchReportModel(DateTime reportDate)
        {
            var cachedmodel = _cache.Get<RiskBatchModel>("RiskBatchReport");
            if (cachedmodel != null)
                return cachedmodel;



            var model = new RiskBatchModel();

            // Get data
            var statsTask = _database.GetBatchSummaryStatsAsync(reportDate, reportDate.AddDays(1));

            // Get error date
            var errorTask = GetBatchSummariesAsync(reportDate);

            Task[] tasks = {statsTask, errorTask};
            Task.WaitAll(tasks);

            // Get list of sites (order descending in length so that matches are done correctly)
            var sites = AllCountries;

            // Convert to Dictionary
            var runs = GetResults(statsTask.Result, sites, reportDate, errorTask.Result);

            // Run through each site
            foreach (var site in sites.OrderBy(s => s))
            {
                var group = new RiskBatchGroup(site);
                model.Items.Add(group);

                // Get site runs
                List<RiskBatchResult> siteRuns;
                if (!runs.TryGetValue(site, out siteRuns))
                {
                    siteRuns = new List<RiskBatchResult>();
                    siteRuns.Add(new RiskBatchResult(site, reportDate));
                }

                runs.Remove(site);

                // Add site runs to site group
                foreach (var item in siteRuns)
                    group.Items.Add(item);
            }

            // Create other batch sections
            if (runs.Count > 0)
            {
                foreach (var set in runs)
                {
                    var other = new RiskBatchGroup(set.Key);
                    model.Items.Add(other);
                    foreach (var item in set.Value)
                        other.Items.Add(item);
                }
            }

            // Remove discarded runs
            foreach (var item in model.Items)
            {
                var nonOverwrittenResults = item.Items.Where(x => x.State != StateItemState.Unknown).ToList();
                item.Items.Clear();
                foreach (var nonOverwrittenResult in nonOverwrittenResults)
                {
                    item.Items.Add(nonOverwrittenResult);
                }
            }

            _cache.Add("RiskBatchReport", model);

            return model;
        }

        private Dictionary<string, List<RiskBatchResult>> GetResults(List<ExtendedBatchSummary> results, string[] sites,
            DateTime reportDate, List<BatchSummary> errorData)
        {
            var result = new Dictionary<string, List<RiskBatchResult>>();
            if (results != null)
            {
                foreach (var row in results)
                {
                    // Determine site
                    var resource = row.Resource;
                    var site = sites.FirstOrDefault(s =>
                        resource.IndexOf(s, StringComparison.CurrentCultureIgnoreCase) >= 0);
                    if (site == null) site = resource;

                    var target = GetOrAdd(result, site, reportDate);
                    target.Update(row, site);
                    target.UpdateErrors(row, errorData);
                }
            }

            // Update status of all batches that were rerun.
            foreach (var list in result.Values)
            {
                if (list.Count <= 1) continue;
                list.Sort(new RiskBatchSort());
                for (var index = 0; index < list.Count - 1; index++)
                {
                    // Only mark results as discard if there is a later run of the same name
                    if (list[index].BatchName != list[index + 1].BatchName) continue;
                    list[index].Text = "Results discarded";
                    list[index].State = StateItemState.Unknown;
                    list[index + 1].IsRerun = true;
                }
            }

            return result;
        }

        private class RiskBatchSort : IComparer<RiskBatchResult>
        {
            public int Compare(RiskBatchResult x, RiskBatchResult y)
            {
                if (x.BatchName != y.BatchName)
                {
                    return x.BatchName.CompareTo(y.BatchName);
                }

                return x.Started.CompareTo(y.Started);
            }
        }

        private RiskBatchResult GetOrAdd(Dictionary<string, List<RiskBatchResult>> lookup, string site,
            DateTime reportDate)
        {
            List<RiskBatchResult> list;
            if (!lookup.TryGetValue(site, out list))
            {
                list = new List<RiskBatchResult>();
                list.Add(new RiskBatchResult(site, reportDate));
                lookup.Add(site, list);
            }

            var target = list.LastOrDefault();
            if (target == null || target.Completed > DateTime.MinValue)
            {
                target = new RiskBatchResult(site, reportDate);
                list.Add(target);
            }

            return target;
        }

        public DateTime GetPreviousWorkday()
        {
            var result = DateTime.Today.AddDays(-1);
            while (result.DayOfWeek == DayOfWeek.Saturday || result.DayOfWeek == DayOfWeek.Sunday)
                result = result.AddDays(-1);
            return result;
        }

        private async Task<List<BatchSummary>> GetBatchSummariesAsync(DateTime valuationDate)
        {
            return await Task.Factory.StartNew(() =>
            {
                var response = _gateway.Get("managementinterface", $"batch/{valuationDate:yyyy-MM-dd}/summary",
                    CancellationToken.None);

                if (!response.Successfull || response.Content?.Payload == null)
                    return new List<BatchSummary>();

                var payload = response.Content.GetPayloadAsString();
                var data = JsonConvert.DeserializeObject<List<BatchSummary>>(payload);

                return data;
            }).ConfigureAwait(false);
        }
    }

    public class RiskBatchModel
    {
        public RiskBatchModel()
        {
            Items = new List<RiskBatchGroup>();
        }

        public List<RiskBatchGroup> Items { get; private set; }
    }

    public class RiskBatchGroup
    {
        public RiskBatchGroup(string name)
        {
            Name = name;
            Items = new List<RiskBatchResult>();
        }

        public string Name { get; set; }

        public int TotalRuns => Items.Count;

        [JsonIgnore]
        public int CompleteRuns
        {
            get { return Items.Count(x => x.State == StateItemState.Okay); }
        }

        public List<RiskBatchResult> Items { get; set; }

        [JsonIgnore]
        public List<RiskBatchResult> IncompleteItems
        {
            get { return Items.Where(x => x.State != StateItemState.Okay).ToList(); }
        }
    }

    public class RiskBatchResult : StateItem
    {
        public RiskBatchResult(string site, DateTime date)
        {
            Resource = site;
            Date = date;
            TargetCompletion = date.AddHours(31);
            if (TargetCompletion < DateTime.Now)
            {
                Text = "SLA Breached";
                State = StateItemState.Warn;
            }
            else
            {
                Text = "No results";
                State = StateItemState.Warn;
            }
        }

        public void Update(ExtendedBatchSummary row, string site)
        {
            CorrelationId = row.CorrelationId;
            Started = row.StartUtc.ToLocalTime();
            TimeTakenMs = (int)(row.EndUtc - row.StartUtc).TotalMilliseconds;
            Completed = Started.AddMilliseconds(TimeTakenMs);
            Resource = row.Resource;
            Version = row.ControllerVersion;
            Site = site;
            State = StateItemState.Okay;
            Link = "~/Request/Summary?correlationId=" + CorrelationId;

            if (row.Successfull)
            {
                Text = "Complete";
            }
            else
            {
                var totalRequests = row.CalculationPricingRequestResults.Values.Sum(v => v.Item2);
                var totalSuccess = row.CalculationPricingRequestResults.Values.Sum(v => v.Item1);
                Text = string.Format("{0} pass / {1}", totalSuccess, totalRequests);
                State = StateItemState.Error;
            }

            Name = row.Name.MaxLength(30);

            Trades = row.Trades;
            PricingRequests = row.PricingRequests;
            MarketDataRequests = row.MarketDataRequests;
            RiskDataRequests = row.RiskDataRequests;

            //Time = string.Format("{0:ddd HH:mm}-{1:ddd HH:mm}", Started, Completed);
            Time = string.Format("{0:ddd HH:mm}", Completed);
            Duration = string.Format("{0}", FormatTimeTaken());


            if (State != StateItemState.Okay) return;
            
            // Some additional rules that affect batch results
            if (Trades <= 0)
            {
                Text = "No Trades";
                State = StateItemState.Warn;
                return;
            }

            if (PricingRequests <= 0)
            {
                Text = "No Pricing Requests";
                State = StateItemState.Warn;
                return;
            }

            if (RiskDataRequests < PricingRequests)
            {
                Text = "Not enough risk data calls";
                State = StateItemState.Warn;
                return;
            }
        }
        

        public void UpdateErrors(ExtendedBatchSummary row, List<BatchSummary> errorData)
        {
            var batchName = $"{Site.ToUpper()} - {Name.ToUpper()}";
            var errors = errorData.Where(x => x.LegalEntity.ToUpper() == batchName).ToList();

            if (!errors.Any())
            {
                ErrorCount = 0;
                return;
            }

            if (errors.Count == 1)
            {
                ErrorCount = errors.First()?.TotalErrorCount;
                return;
            }

            var error = errors.FirstOrDefault(x => x.LegalEntity.ToUpper() == batchName);
            ErrorCount = error?.TotalErrorCount;
        }

        public Guid CorrelationId { get; private set; }

        public DateTime Date { get; private set; }

        public string Resource { get; private set; }

        public string BatchName => $"{Site}-{Name}";

        public string Version { get; private set; }

        public string Site { get; private set; }

        public DateTime TargetCompletion { get; private set; }

        public DateTime Started { get; private set; }

        public string StartedFormatted
        {
            get { return Started == DateTime.MinValue ? string.Empty : Started.ToString("ddd HH:mm"); }
        }

        public DateTime Completed { get; private set; }

        public string CompletedFormatted
        {
            get { return Completed == DateTime.MinValue ? string.Empty : Completed.ToString("ddd HH:mm"); }
        }

        public long TimeTakenMs { get; private set; }

        public bool IsRerun { get; set; }

        public string Duration { get; set; }

        public long? Trades { get; set; }
        public long? PricingRequests { get; set; }
        public long? MarketDataRequests { get; set; }
        public long? RiskDataRequests { get; set; }

        private string FormatTimeTaken()
        {
            return TimeTakenMs.FormatTimeTaken();
        }

        public int? ErrorCount { get; set; }
    }

    public class BatchSummary
    {
        public string LegalEntity { get; set; }
        public string Controller { get; set; }
        public string ControllerVersion { get; set; }
        public DateTime? ValuationDate { get; set; }
        public int ExecutionCount { get; set; }
        public DateTime StartTime { get; set; }
        public long Duration { get; set; }
        public int TradeCount { get; set; }
        public int FatalErrorCount { get; set; }
        public int TotalErrorCount { get; set; }
        public Guid RequestCorrelationId { get; set; }

        public string TimeTakenMs => TimeSpan.FromMilliseconds(Duration).Humanize();

        public string GetBatchName(string site)
        {
            return LegalEntity
                .Replace(site, "")
                .Replace(site.ToUpper(), "")
                .Replace("-", "")
                .Trim();
        }
    }

    public class BatchDetail
    {
        public string LegalEntity { get; set; }
        public DateTime ValuationDate { get; set; }
        public IList<BatchExecution> BatchExecutions { get; set; }

        public BatchDetail()
        {
            BatchExecutions = new List<BatchExecution>();
        }

        public string BatchName => LegalEntity
            .Replace("-", " - ")
            .Replace("_", " ");
    }

    public class BatchExecution
    {
        public BatchSummary Summary { get; set; }
        public IList<BatchIssues> Issues { get; set; }
        public long BatchStatId { get; set; }

        public BatchExecution()
        {
            Issues = new List<BatchIssues>();
        }
    }

    public class BatchIssues
    {
        public BatchCommentDto Issue { get; set; }
        public IList<BatchCommentDto> Comments { get; set; }
        public Dictionary<Guid, string> Occurences { get; set; }
        public int OccurenceCount => Occurences.Count;

        public BatchIssues()
        {
            Comments = new List<BatchCommentDto>();
            Occurences = new Dictionary<Guid, string>();
        }
    }

    public class BatchCommentDto
    {
        public int Id { get; set; }
        public int BatchStatId { get; set; }
        public Guid RequestCorrelationId { get; set; }
        public Guid ParentRequestCorrelationId { get; set; }
        public int BatchCommentType { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDescription { get; set; }
        public string ReportedBy { get; set; }
        public string Controller { get; set; }
        public string ControllerVersion { get; set; }
        public DateTime ReportedAt { get; set; }
        public string Resource { get; set; }
        public int? ParentBatchCommentId { get; set; }
    }
}