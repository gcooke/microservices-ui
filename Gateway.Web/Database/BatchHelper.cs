using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Gateway.Web.Models.Home;
using RestSharp.Extensions;

namespace Gateway.Web.Database
{
    public class BatchHelper
    {
        private const string AllCountries = "Botswana,Ghana,Kenya,Mauritius_IBD,Mauritius_Onshore,Mozambique,South_Africa,Seychelles,Tanzania,Tanzania_NBC,Uganda,Zambia";
        private readonly IGatewayDatabaseService _database;

        public BatchHelper(IGatewayDatabaseService database)
        {
            _database = database;
        }

        public Task<RiskBatchModel> GetRiskBatchReportModel(DateTime reportDate)
        {
            var model = new RiskBatchModel();

            // Get data
            var results = _database.GetBatchSummaryStats(reportDate, reportDate.AddDays(1));

            // Get list of sites (order descending in length so that matches are done correctly)
            var sites = AllCountries.Split(',').OrderByDescending(s => s.Length).ToArray();

            // Convert to Dictionary
            var runs = GetResults(results, sites, reportDate);

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
                    nonOverwrittenResult.IsRerun = false;
                    item.Items.Add(nonOverwrittenResult);
                }
            }

            return Task.FromResult(model);
        }

        private Dictionary<string, List<RiskBatchResult>> GetResults(List<ExtendedBatchSummary> results, string[] sites, DateTime reportDate)
        {
            var result = new Dictionary<string, List<RiskBatchResult>>();
            if (results != null)
            {
                var defaultBatchName = "Counterparty";
                var defaultQuotesName = "Quotes";

                foreach (var row in results)
                {
                    // Determine site
                    var resource = row.Resource;
                    var site = sites.FirstOrDefault(s => resource.IndexOf(s, StringComparison.CurrentCultureIgnoreCase) >= 0);
                    if (site == null) site = resource;

                    var target = GetOrAdd(result, site, reportDate);
                    target.Update(row, site, defaultBatchName, defaultQuotesName);
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
                    if (list[index].Resource != list[index + 1].Resource) continue;
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
                if (x.Resource != y.Resource)
                {
                    return x.Resource.CompareTo(y.Resource);
                }
                return x.Started.CompareTo(y.Started);
            }
        }

        private RiskBatchResult GetOrAdd(Dictionary<string, List<RiskBatchResult>> lookup, string site, DateTime reportDate)
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

        public string Name { get; private set; }
        public List<RiskBatchResult> Items { get; private set; }
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

        public void Update(ExtendedBatchSummary row, string site, string defaultBatchName, string defaultQuotesName)
        {
            CorrelationId = row.CorrelationId;
            Started = row.StartUtc.ToLocalTime();
            TimeTakenMs = (int)(row.EndUtc - row.StartUtc).TotalMilliseconds;
            Completed = Started.AddMilliseconds(TimeTakenMs);
            Resource = row.Resource;
            Version = row.ControllerVersion;
            Site = site;
            State = StateItemState.Okay;

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

            // Determine name
            if (Resource != site) // i.e. sub-calc
            {
                var siteBoundedByHyphens = Resource.IndexOf("-" + site + "-", StringComparison.CurrentCultureIgnoreCase);
                if (siteBoundedByHyphens >= 0)
                {
                    // [Type]-[Site]-[Date:yyyy-MM-dd]
                    var displayName = Resource.Substring(0, Resource.IndexOf("-"));
                    if (displayName.ToUpper() == "QUOTES")
                        Name = defaultQuotesName;
                    else
                        Name = displayName.ToPascalCase(true, CultureInfo.CurrentUICulture);
                }
                else
                {
                    Name = defaultBatchName;
                }
            }
            else
            {
                Name = Resource;
            }
            
            Time = string.Format("{0:ddd HH:mm}-{1:ddd HH:mm}", Started, Completed);
        }

        public Guid CorrelationId { get; private set; }

        public DateTime Date { get; private set; }

        public string Resource { get; private set; }

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
    }

}