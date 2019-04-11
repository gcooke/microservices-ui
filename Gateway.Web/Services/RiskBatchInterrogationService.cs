using System.Linq;
using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MIT.Cube.Impl;
using Bagl.Cib.MIT.Cube.Utils;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.IoC.Models;
using Gateway.Web.Database;
using Gateway.Web.Models.Interrogation;
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

                    var count = 0;
                    foreach (var issueTracker in issueTrackersForBatch)
                    {
                        var issues = issueTracker.Identify(model, gatewayDb, pnrFoDb, batch);
                        foreach (var issue in issues.IssueList)
                        {
                            var description = issue.Description;
                            if (issue.HasRemediation)
                                description += "<br/><br/>REMEDIATION: " + issue.Remediation;

                            if (issue.MonitoringLevel >= model.MinimumLevel)
                                cube.AddRow(new object[] { issue.MonitoringLevel, description });

                            count++;
                        }

                        if (issues.IssueList.Any(x => !x.ShouldContinueCheckingIssues))
                        {
                            break;
                        }
                    }

                    if (cube.Rows == 0)
                        cube.AddRow(new object[] { MonitoringLevel.Ok, $"Batch looks okay - {count} validation tests passed" });

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