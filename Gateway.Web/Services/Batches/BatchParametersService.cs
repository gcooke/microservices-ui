using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models.Batches;
using Gateway.Web.Services.Batches.Interfaces;
using Newtonsoft.Json;

namespace Gateway.Web.Services.Batches
{
    public class BatchParametersService : IBatchParametersService
    {
        private readonly IGateway _gateway;

        public BatchParametersService(IGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<BatchSettingsReport> PopulateDifferences(BatchSettingsReport report)
        {
            var defaults = await LoadDefaultSettings(report);

            foreach (var item in report.Batches)
            {
                if (item.Echo == null) continue;

                if (item.Echo.Parameters == null)
                {
                    item.Differences.Add(item.Echo.ApplicationName);
                    continue;
                }

                if (!defaults.TryGetValue(item.BatchName, out var parameters))
                {
                    item.Differences.Add("Could not find QA defaults");
                    continue;
                }

                item.Differences = GetDifferences(item, parameters);
            }

            return report;
        }

        private List<string> GetDifferences(BatchItem item, List<CalcStateParameter> parameters)
        {
            var result = new List<string>();
            var configuration = item.Echo.Parameters.Categories.FirstOrDefault(c => c.Name == "Configuration");
            if (configuration == null)
            {
                result.Add("No parameters found in echo");
                return result;
            }

            foreach (var entry in configuration.Entries)
            {
                // Ignore these entries
                if (entry.Key == "Name" || entry.Key == "Description" || entry.Key == "Parent") continue;

                var target = parameters.FirstOrDefault(p => string.Equals(p.Name, entry.Key, StringComparison.CurrentCultureIgnoreCase));

                if (target == null)
                {
                    result.Add($"No QA setting for '{entry.Key}'");
                }
                else if (entry.Key == "ReportingCurrency" && target.Value == "SITE")
                {
                    // Ignore site differences on reporting currency (this is by design)
                    continue;
                }
                else if (target.Value != entry.Value)
                {
                    result.Add($"{entry.Key} value of '{entry.Value}' differs from QA default of '{target.Value}'");
                }
            }

            // Special market data map check
            var marketDataSection = item.Echo.Parameters.Categories.FirstOrDefault(c => c.Name == "Market Data");
            var marketDataMap = marketDataSection?.Entries.FirstOrDefault(e => string.Equals(e.Key.Trim(), "Market Data Map", StringComparison.CurrentCultureIgnoreCase));
            if (marketDataMap != null)
            {
                var desired = parameters.FirstOrDefault(p => string.Equals(p.Name, "MarketDataMapName", StringComparison.CurrentCultureIgnoreCase));
                if (!string.Equals(desired?.Value, marketDataMap.Value, StringComparison.CurrentCultureIgnoreCase))
                {
                    result.Add($"{marketDataMap.Key} value of '{marketDataMap.Value}' differs from QA default of '{desired?.Value}'");
                }
            }

            return result;
        }

        private async Task<Dictionary<string, List<CalcStateParameter>>> LoadDefaultSettings(BatchSettingsReport report)
        {
            var query = "Batch/Parameters";
            var controller = "StaticData";

            var content = string.Join(",", report.Batches.Select(b => b.BatchName).Distinct());
            var request = await _gateway.Put<string, string>(controller, query, content);
            var parameters = JsonConvert.DeserializeObject<CalcStateParameters[]>(request.Body);
            var result = new Dictionary<string, List<CalcStateParameter>>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var item in parameters)
                result[item.BatchName] = item.Items;

            return result;
        }
    }
}