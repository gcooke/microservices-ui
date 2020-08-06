using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MIT.Cube.Impl;
using System.Web.Mvc;
using Absa.Cib.MIT.TaskScheduling.Models;
using Bagl.Cib.MIT.Cube.Actions;
using Bagl.Cib.MIT.Cube.Utils;
using Bagl.Cib.MIT.IO;
using Bagl.Cib.MIT.IoC;
using Gateway.Web.Database;
using Gateway.Web.Helpers;
using Gateway.Web.Models.Interrogation;
using Gateway.Web.Models.Request;
using Gateway.Web.Services.Batches.Interrogation.Issues.BatchIssues;
using Gateway.Web.Services.Batches.Interrogation.Models;
using Gateway.Web.Services.Batches.Interrogation.Models.Enums;
using Gateway.Web.Services.Batches.Interrogation.Services.BatchService;
using Gateway.Web.Services.Batches.Interrogation.Services.IssueService;
using Gateway.Web.Utils;
using Newtonsoft.Json;
using Unity.Interception.Utilities;

namespace Gateway.Web.Services
{
    public class RiskBatchInterrogationService : IRiskBatchInterrogationService
    {
        private readonly IFileService _fileService;
        private readonly IBatchService _batchService;
        private readonly IIssueTrackerService _issueTrackerService;
        private readonly string _connectionString;
        private readonly string _pnrFoConnectionString;

        public RiskBatchInterrogationService(ISystemInformation information,
            IFileService fileService,
            IBatchService batchService, IIssueTrackerService issueTrackerService)
        {
            _fileService = fileService;
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
                            var issues = issueTracker.Identify(model, gatewayDb, pnrFoDb, batch, run);
                            foreach (var issue in issues.IssueList)
                            {
                                var description = issue.Description;
                                var remediation = (string)null;
                                if (issue.HasRemediation)
                                    remediation += "<b>REMEDIATION:</b><br/> " + issue.Remediation;

                                if (issue.MonitoringLevel >= model.MinimumLevel)
                                    cube.AddRow(new object[] { issue.MonitoringLevel, description, remediation });
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

        public XvaReportModel GetXvaReport(string correlationId, string reportDateString, string allRows)
        {
            var model = new XvaReportModel();
            model.IncludeAllRows = allRows == "true";
            model.Date = string.IsNullOrEmpty(reportDateString) ? DateTime.Today.ToString(XvaReportModel.DateFormat) : reportDateString;
            model.CorrelationId = string.IsNullOrEmpty(correlationId) ? Guid.Empty : Guid.Parse(correlationId);

            using (var gatewayDb = new GatewayEntities(_connectionString))
            {
                // Populate available reports
                PopulateAvailableReports(gatewayDb, model);

                // Try find correlation id if none provided
                if (model.CorrelationId == Guid.Empty)
                {
                    model.CorrelationId = model.Reports.FirstOrDefault(r => r.BusinessDate == model.Date)?.CorrelationId ?? Guid.Empty;
                }

                // Get report details
                if (model.CorrelationId == Guid.Empty) return model;

                // Get first level children payloads
                var cubes = new List<ICube>();
                foreach (var child in gatewayDb.Requests.Where(r => r.ParentCorrelationId == model.CorrelationId))
                {
                    var responses = gatewayDb.Payloads.Where(p => p.CorrelationId == child.CorrelationId).ToList();
                    var response = responses.FirstOrDefault(r => r.Direction == "Response");
                    if (response == null) continue;

                    if (!string.IsNullOrEmpty(response.Server))
                        response.Data = PayloadFileRetrieval.GetPayloadFromSever(_fileService, response);

                    if (response.Data == null || response.Data.Length == 0) continue;

                    var data = new PayloadData(response);
                    var bytes = data.GetBytes();

                    var str = Encoding.UTF8.GetString(bytes);
                    var m = new XvaResultModel(str, correlationId, 0);
                    if (m.BatchStatistics != null)
                        cubes.Add(m.BatchStatistics.Cube);
                }
                if (cubes.Count == 0) return model;

                // Merge cubes
                var cube = MergeMany.Execute(cubes.ToArray());

                // Format result cube and generate stats
                var stats = new Stats();
                cube.RemoveColumn("Output Tag");
                var resultCube = cube.CloneStructure();
                foreach (var row in cube.GetRows())
                {
                    var hasError = IncrementStats(stats, row);
                    if (model.IncludeAllRows || hasError)
                        resultCube.AddRow(row);
                }
                resultCube.SetAttribute("Counterparties", stats.Counterparties.Count);
                resultCube.SetAttribute("Calculations", stats.Rows);
                resultCube.SetAttribute("Errors", stats.Errors);

                // Render statistics
                model.BatchStatistics = new CubeModel(resultCube, "Statistics");
            }

            return model;
        }

        private bool IncrementStats(Stats stats, IRow arg)
        {
            var hasError = false;
            stats.Rows++;
            if (arg.GetValueOrDefault<bool>("Has Errors") == true)
            {
                stats.Errors++;
                hasError = true;
            }
            if (arg.GetValueOrDefault<bool>("Has Data") == false)
            {
                hasError = true;
            }

            stats.Counterparties.Add(arg.GetStringValueOrDefault("Scenario"));
            return hasError;
        }

        private void PopulateAvailableReports(GatewayEntities gateway, XvaReportModel model)
        {
            var configurations = gateway.RequestConfigurations.Where(c => c.Name.StartsWith("XVA - Nightly Run"));
            foreach (var configuration in configurations)
            {
                // Get site name
                var site = string.Empty;
                var args = JsonConvert.DeserializeObject<IList<Argument>>(configuration.Arguments);
                var target = args.SingleOrDefault(x => x.Key.ToLower() == "site");
                if (target != null)
                {
                    site = target.FormatValue;
                }
                var scheduleId = configuration.Schedules.FirstOrDefault()?.ScheduleId ?? -1;
                if (scheduleId < 0) continue;

                // Get runs
                var runs = gateway.ScheduledJobs.Where(j => j.ScheduleId == scheduleId).OrderByDescending(j => j.Id).Take(10);
                foreach (var item in runs)
                {
                    model.AddReport(item.StartedAt, item.BusinessDate, site, item.RequestId);
                }
            }
        }

        private ICube CreateReportCube(Batch batch, BatchRun run)
        {
            var cube = new CubeBuilder()
                .AddColumn(" ", ColumnType.Int)
                .AddColumn("Description")
                .AddColumn("Remediation")
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

        public class Stats
        {
            public Stats()
            {
                Counterparties = new HashSet<string>();
            }
            public int Rows { get; set; }
            public int Errors { get; set; }
            public HashSet<string> Counterparties { get; }
        }
    }
}