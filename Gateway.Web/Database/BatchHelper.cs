using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models.Home;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bagl.Cib.MIT.Redis.Caching;

namespace Gateway.Web.Database
{
    public class BatchHelper : IBatchHelper
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

        public async Task<RiskBatchModel> GetRiskBatchReportModelAsync(DateTime reportDate)
        {
            const string key = @"{BatchReporting}:RiskBatchReport";
            var cachedmodel = _cache.Get<RiskBatchModel>(key);
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

            _cache.Add(key, model);

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
}