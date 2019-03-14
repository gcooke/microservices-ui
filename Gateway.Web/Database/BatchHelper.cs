using Bagl.Cib.MSF.ClientAPI.Gateway;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MIT.Redis.Caching;
using Bagl.Cib.MSF.ClientAPI.Model;

namespace Gateway.Web.Database
{
    public class BatchHelper : IBatchHelper
    {
        private const string AllCountries = "Botswana,Ghana,Kenya,Mauritius_IBD,Mauritius_Onshore,Mozambique,South_Africa,Seychelles,Tanzania,Tanzania_NBC,Uganda,Zambia";

        private readonly IGateway _gateway;
        private readonly IRedisCache _cache;
        private readonly ILogger _logger;

        public BatchHelper(ILoggingService loggingService, IGateway gateway, IRedisCache cache)
        {
            _logger = loggingService.GetLogger(this);
            _gateway = gateway;
            _cache = cache;
        }

        public async Task<RiskBatchModel> GetRiskBatchReportModel(DateTime reportDate, string targetSite)
        {
            var model = new RiskBatchModel();
            model.Site = targetSite;
            model.ReportDate = reportDate;
            model.ShowOverwrittenResults = false;

            // Get data            
            var cube = await GetRiskBatchReport(reportDate).ConfigureAwait(false);

            // Get list of sites (order descending in length so that matches are done correctly)
            var sites = AllCountries.Split(',').OrderByDescending(s => s.Length).ToArray();
            model.AvailableSites.AddRange(sites.OrderBy(s => s));

            // Convert to Dictionary
            var runs = GetResults(cube, sites, reportDate);

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
                foreach (var item in siteRuns.OrderBy(s => s.Started).ThenBy(s => s.Name))
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

            // Trim to desired site
            if (targetSite != "All")
            {
                model.Items.RemoveAll(r => r.Name != targetSite);
            }

            return model;
        }

        public async Task<ICube> GetRiskBatchReport(DateTime date)
        {
            _logger.InfoFormat("GetRiskBatchReport({0}, {1})", date);

            var report = string.Format("batch/{0:yyyyMMdd}/report", date);
            var request = new Get("ManagementInterface") { Query = report };
            var response = await _gateway.Invoke<ICube>(request).ConfigureAwait(false);

            if (response.Successfull)
            {
                return response.Body;
            }
            throw new InvalidOperationException(response.Message);
        }

        private Dictionary<string, List<RiskBatchResult>> GetResults(ICube cube, string[] sites, DateTime reportDate)
        {
            var result = new Dictionary<string, List<RiskBatchResult>>(StringComparer.CurrentCultureIgnoreCase);
            if (cube != null)
            {
                foreach (var row in cube.GetRows())
                {
                    // Determine site
                    var site = row["Site"].ToString();

                    var target = GetOrAdd(result, site, reportDate);
                    target.Update(row);
                }
            }

            return result;
        }

        private RiskBatchResult GetOrAdd(Dictionary<string, List<RiskBatchResult>> lookup, string site, DateTime reportDate)
        {
            List<RiskBatchResult> list;
            if (!lookup.TryGetValue(site, out list))
            {
                list = new List<RiskBatchResult>();
                lookup.Add(site, list);
            }

            var target = new RiskBatchResult(site, reportDate);
            list.Add(target);
            return target;
        }
    }
}