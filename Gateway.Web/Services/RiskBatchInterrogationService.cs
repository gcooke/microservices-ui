using System.Linq;
using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MIT.Cube.Impl;
using Bagl.Cib.MIT.Cube.Utils;
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.IoC.Models;
using Gateway.Web.Database;
using Gateway.Web.Database;
using Gateway.Web.Models.Interrogation;
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
                    .Select(x => new SelectListItem() { Value = x, Text = x});

                model.TradeSources.AddRange(tradeSources);
                model.BatchTypes.AddRange(batchTypes);
            }
        }

        public void Analyze(InterrogationModel model)
        {
            model.Report.TablesList.Clear();
            var batches = _batchService
                .GetBatchesForDate(model)
                .OrderBy(x => x.BatchType)
                .ToList();

            using (var pnrFoDb = new Entities(_pnrFoConnectionString))
            using (var gatewayDb = new GatewayEntities(_connectionString))
            {
                foreach (var batch in batches)
                {
                    var latestRun = batch.ActualOccurrences.OrderByDescending(x => x.StartedAt).FirstOrDefault();
                    var batchLabel = $"{batch}";
                    if (latestRun != null)
                        batchLabel += $" [LATEST RUN = {latestRun.CorrelationId}]";

                    var cube = CreateReportCube(batchLabel);
                    var issueTrackersForBatch = _issueTrackerService.GetIssueTrackersForBatch(batch.BatchType);

                    foreach (var issueTracker in issueTrackersForBatch)
                    {
                        var issues = issueTracker.Identify(gatewayDb, pnrFoDb, batch);
                        foreach (var issue in issues.IssueList)
                        {
                            var description = issue.Description;
                            if (issue.HasRemediation)
                                description += "<br/><br/>REMEDIATION: " + issue.Remediation;
                            cube.AddRow(new object[] { issue.MonitoringLevel, description });
                        }

                        if (issues.IssueList.Any(x => !x.ShouldContinueCheckingIssues))
                        {
                            break;
                        }
                    }

                    model.Report.Add(cube);
                }
            }
        }

        private ICube CreateReportCube(string title)
        {
            var cube = new CubeBuilder()
                .AddColumn(" ", ColumnType.Int)
                .AddColumn("Description")
                .Build();

            cube.SetAttribute("Title", title);
            return cube;
        }
    }
}