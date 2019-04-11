using System.Linq;
using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MIT.Cube.Impl;
using Bagl.Cib.MIT.Cube.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.IoC.Models;
using Gateway.Web.Database;
using Gateway.Web.Database;
using Gateway.Web.Models.Interrogation;
using Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues;
using Gateway.Web.Services.Batches.Interrogation.Models;
using Gateway.Web.Services.Batches.Interrogation.Models.Enums;
using Gateway.Web.Services.Batches.Interrogation.Services.BatchService;
using Gateway.Web.Services.Batches.Interrogation.Services.IssueService;

namespace Gateway.Web.Services
{
    public class RiskBatchInterrogationService : IRiskBatchInterrogationService
    {
        private readonly IBatchService _batchService;
        private readonly IIssueTrackerService _issueTrackerService;
        private readonly string _connectionString;
        private readonly string _pnrFoConnectionString;

        public RiskBatchInterrogationService(ISystemInformation information,
            IBatchService batchService, IIssueTrackerService issueTrackerService)
        {
            _batchService = batchService;
            _issueTrackerService = issueTrackerService;
            _connectionString = information.GetConnectionString("GatewayDatabase", "Database.PnRFO_Gateway");
            _pnrFoConnectionString = information.GetConnectionString("PnRFODatabase", "Database.PnRFO");
        }

        public void PopulateLookups(InterrogationModel model)
        {
            using (var db = new GatewayEntities(_connectionString))
            {
                var tradeSources = db
                    .RiskBatchSchedules
                    .Where(x => x.TradeSourceType == "Site")
                    .Select(x => x.TradeSource)
                    .Distinct()
                    .ToList()
                    .Select(x => new SelectListItem() { Value = x, Text = x });

                var batchTypes = db.RiskBatchSchedules
                    .Select(x => x.RiskBatchConfiguration.Type)
                    .Distinct()
                    .ToList()
                    .Select(x => new SelectListItem() { Value = x, Text = x });

                model.TradeSources.AddRange(tradeSources);
                model.BatchTypes.AddRange(batchTypes);
            }
        }

        public void Analyze(InterrogationModel model)
        {
            model.Report.TablesList.Clear();
            model.Tests.Clear();

            var batches = _batchService
                .GetBatchesForDate(model)
                .OrderBy(x => x.BatchType)
                .ToList();

            using (var pnrFoDb = new Entities(_pnrFoConnectionString))
            using (var gatewayDb = new GatewayEntities(_connectionString))
            {
                foreach (var batch in batches)
                {
                    foreach (var run in batch.ActualOccurrences.OrderByDescending(x => x.StartedAt))
                    {
                        var cube = CreateReportCube(batch, run);
                        var issueTrackersForBatch = _issueTrackerService.GetIssueTrackersForBatch(batch.BatchType);
                        var context = new BatchInterrogationContext(run.CorrelationId, gatewayDb, pnrFoDb);

                        foreach (var issueTracker in issueTrackersForBatch)
                        {
                            foreach (var test in issueTracker.GetDescriptions())
                                model.Tests.Add(test);
                            issueTracker.SetContext(context);
                            var issues = issueTracker.Identify(gatewayDb, pnrFoDb, batch, run);
                            foreach (var issue in issues.IssueList)
                            {
                                var description = issue.Description;
                                if (issue.HasRemediation)
                                    description += "<br/><br/>REMEDIATION: " + issue.Remediation;

                                if (issue.MonitoringLevel >= model.MinimumLevel)
                                    cube.AddRow(new object[] { issue.MonitoringLevel, description });
                            }

                            if (issues.IssueList.Any(x => !x.ShouldContinueCheckingIssues))
                            {
                                break;
                            }
                        }

                        if (cube.Rows == 0)
                            cube.AddRow(new object[] { MonitoringLevel.Ok, $"Batch looks okay - validation tests passed" });

                        model.Report.Add(cube);
                    }
                }
            }
        }

        private ICube CreateReportCube(Batch batch, BatchRun run)
        {
            var cube = new CubeBuilder()
                .AddColumn(" ", ColumnType.Int)
                .AddColumn("Description")
                .Build();

            cube.SetAttribute("BatchType", batch.BatchType);
            cube.SetAttribute("TradeSource", batch.TradeSource);
            cube.SetAttribute("Site", batch.Site);
            cube.SetAttribute("TradeSourceType", batch.TradeSourceType);

            if (run.CorrelationId.HasValue)
                cube.SetAttribute("CorrelationId", run.CorrelationId.Value.ToString());

            if (run.ValuationDate.HasValue)
                cube.SetAttribute("ValuationDate", run.ValuationDate.Value.ToString("yyyy-MM-dd"));

            if (run.StartedAt.HasValue)
                cube.SetAttribute("StartedAt", run.StartedAt.Value.ToString("yyyy-MM-dd hh:mm tt"));

            if (run.FinishedAt.HasValue)
                cube.SetAttribute("FinishedAt", run.FinishedAt.Value.ToString("yyyy-MM-dd hh:mm tt"));

            if (!string.IsNullOrWhiteSpace(run.CurrentStatus))
                cube.SetAttribute("CurrentStatus", run.CurrentStatus);
            else
                cube.SetAttribute("CurrentStatus", "Not Started");

            return cube;
        }
    }
}