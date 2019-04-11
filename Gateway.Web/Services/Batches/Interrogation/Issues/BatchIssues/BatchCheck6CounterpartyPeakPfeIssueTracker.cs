using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MIT.StatePublisher.Utils;
using Gateway.Web.Database;
using Gateway.Web.Models.Interrogation;
using Gateway.Web.Services.Batches.Interrogation.Attributes;
using Gateway.Web.Services.Batches.Interrogation.Models;
using Gateway.Web.Services.Batches.Interrogation.Models.Builders;
using Gateway.Web.Services.Batches.Interrogation.Models.Enums;

namespace Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues
{
    public class BatchCheck6CounterpartyPeakPfeIssueTracker : BaseBatchIssueTracker
    {
        public override Models.Issues Identify(InterrogationModel model, GatewayEntities gatewayDb, Entities pnrFoDb, Batch item, BatchRun run)
        {
            var issues = new Models.Issues();
            var t1Date = model.GetValuationDate();
            var t2Date = model.GetPreviousValuationDate();
            var dataSetIdT1 = GetDataset(model, pnrFoDb, t1Date);
            var dataSetIdT2 = GetDataset(model, pnrFoDb, t2Date);

            // Compare previous business days Peak PFE values compared to current date
            var t1 = GetPeakValues(model, pnrFoDb, dataSetIdT1);
            var t2 = GetPeakValues(model, pnrFoDb, dataSetIdT2);
            var diff = GetDifferences(t1, t2);
            var description = $"There are notable differences between Peak PFE values for {t2Date:dd-MMM} vs {t1Date:dd-MMM}: <br/>";
            if (diff.Length > 0)
                new IssueBuilder()
                    .SetDescription(description + diff)
                    .SetMonitoringLevel(MonitoringLevel.Warning)
                    .SetShouldContinueCheckingIssues(false)
                    .BuildAndAdd(issues);

            // Check that all Counterparties that have MTM have Peak PFE
            var dataSetIdPv = GetDataset(model, pnrFoDb, t1Date, "COUNTERPARTY.PV");
            var pv = GetPvValues(model, pnrFoDb, dataSetIdPv);
            var checkPfe = CheckForPfeForPv(pv, t1);
            description = $"These counterparties have a MTM but no PFE: <br/>";
            if (checkPfe.Length > 0)
                new IssueBuilder()
                    .SetDescription(description + checkPfe)
                    .SetMonitoringLevel(MonitoringLevel.Warning)
                    .SetRemediation("Rerun batch for selected counterparties")
                    .SetShouldContinueCheckingIssues(false)
                    .BuildAndAdd(issues);

            return issues;
        }

        private string CheckForPfeForPv(Dictionary<long, decimal> pv, Dictionary<long, decimal> t1)
        {
            var builder = new StringBuilder();
            var count = 0;
            foreach (var key in pv.Keys)
            {
                if (count > 100)
                {
                    builder.AppendLine("<br/><i>only first 100 occurrences reported...</i>");
                    break;
                }
                if (pv[key] != 0 && !t1.ContainsKey(key))
                {
                    if (builder.Length > 0)
                        builder.Append(", ");
                    builder.Append($"{key}");
                    count++;
                }
            }
            return builder.ToString();
        }

        private string GetDifferences(Dictionary<long, decimal> t1, Dictionary<long, decimal> t2)
        {
            var keys = t1.Keys.Union(t2.Keys).Distinct();
            var builder = new StringBuilder();
            var count = 0;
            foreach (var key in keys)
            {
                if (count > 10)
                {
                    builder.AppendLine("<i>only first 10 differences reported...</i>");
                    break;
                }
                if (!t1.ContainsKey(key))
                {
                    builder.AppendLine($"SDSID {key} had a value yesterday but is missing today<br/>");
                    count++;
                    continue;
                }
                if (!t2.ContainsKey(key))
                {
                    continue;
                }
                var t1v = t1[key];
                var t2v = t2[key];
                if (t1v == 0 || t2v == 0) continue;
                var change = ((t2v - t1v) / Math.Abs(t1v)) * 100;
                if (Math.Abs(change) > 90)
                {
                    builder.AppendLine($"SDSID {key} changed from {t2v:N0} to {t1v:N0}<br/>");
                    count++;
                }
                break;
            }
            return builder.ToString();
        }

        private Dictionary<long, decimal> GetPeakValues(InterrogationModel model, Entities pnrFoDb, long dataSetId)
        {
            var result = new Dictionary<long, decimal>();
            var riskTypeId = pnrFoDb.RiskTypes.FirstOrDefault(r => r.Name == "Max PFE_98")?.Id ?? 0;

            var query = pnrFoDb.RiskCompresseds.Where(r => r.DatasetId == dataSetId && r.RiskTypeId == riskTypeId);
            foreach (var risk in query)
            {
                var cube = CubeBuilder.FromBytes(Convert.FromBase64String(risk.RiskValue));
                foreach (var row in cube.GetRows())
                {
                    var sdsId = row.GetValue<long>("SDSID");
                    var value = row.GetValue<decimal>("Value");
                    if (sdsId.HasValue && value.HasValue)
                        result[sdsId.Value] = value.Value;
                }
            }

            return result;
        }

        private Dictionary<long, decimal> GetPvValues(InterrogationModel model, Entities pnrFoDb, long dataSetId)
        {
            var result = new Dictionary<long, decimal>();
            var riskTypeId = pnrFoDb.RiskTypes.FirstOrDefault(r => r.Name == "PV")?.Id ?? 0;

            var query = pnrFoDb.RiskCompresseds.Where(r => r.DatasetId == dataSetId && r.RiskTypeId == riskTypeId && r.TradeId == null);
            foreach (var risk in query)
            {
                var cube = CubeBuilder.FromBytes(Convert.FromBase64String(risk.RiskValue));
                foreach (var row in cube.GetRows())
                {
                    var sdsId = row.GetValue<long>("SDSID");
                    var value = row.GetValue<decimal>("Value");
                    if (sdsId.HasValue && value.HasValue)
                        result[sdsId.Value] = value.Value;
                }
            }

            return result;
        }

        private long GetDataset(InterrogationModel model, Entities pnrFoDb, DateTime date, string batchName = null)
        {
            var name = batchName ?? model.BatchType;
            var leid = pnrFoDb.LegalEntities.FirstOrDefault(le => le.Name == model.TradeSource)?.Id;
            return pnrFoDb.Datasets.FirstOrDefault(d => d.NamePrefix == name && d.BusinessDate == date && d.LegalEntityId == leid)?.Id ?? 0;
        }

        public override IEnumerable<string> GetDescriptions()
        {
            yield return "Compare PFE Peak to T-2 values";
        }

        public override int GetSequence()
        {
            return 6;
        }
    }
}