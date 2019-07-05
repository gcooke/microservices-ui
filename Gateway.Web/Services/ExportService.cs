using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MIT.StatePublisher.Utils;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models.Export;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Services
{
    public class ExportService : IExportService
    {
        private readonly IGateway _gateway;
        private readonly ILogger _logger;
        private readonly string ControllerName = "Export";

        public ExportService(IGateway gateway, ILoggingService loggingService)
        {
            _gateway = gateway;
            _logger = loggingService.GetLogger(this);
            //For testing
            _gateway.SetGatewayUrlForService(ControllerName, "http://localhost:7000/");
        }

        public ExportUpdate CreateExport(ExportUpdate insert)
        {
            var query = "";
            var response = _gateway.Put<string, string>(ControllerName, query, insert.Serialize()).Result;

            if (response.Successfull)
                return insert;
            else
            {
                throw new Exception(response.Message);
            }
        }

        public ICube FetchCube(string query)
        {
            ICube cube = null;

            var resultTask = _gateway.Get<ICube>(ControllerName, query).GetAwaiter().GetResult();
            if (resultTask.Successfull)
                cube = resultTask.Body;
            else
                throw new Exception(resultTask.Message);

            return cube;
        }

        public IList<ExportCRONGroup> FetchExports(DateTime date)
        {
            var query = $@"Exports/Fetch/{date.ToString("yyyy-MM-dd")}";
            var cube = FetchCube(query);

            IList<ExportCRONGroup> result = ConvertCubeToExportCRONGroup(cube);

            return result;
            var groups = new List<ExportCRONGroup>();
            var exports = new List<FileExport>();
            exports.Add(new FileExport()
            {
                Name = "Test 1",
                Status = "Succeeded",
                Type = "CubeToCsv"
            });
            exports.Add(new FileExport()
            {
                Name = "Test 2",
                Status = "Succeeded",
                Type = "CubeToCsv"
            });
            var group = new ExportCRONGroup()
            {
                FileExports = exports,
                GroupName = CronExpressionDescriptor.ExpressionDescriptor.GetDescription("0 9 * * MON-FRI")
            };
            groups.Add(group);

            exports = new List<FileExport>();
            exports.Add(new FileExport()
            {
                Name = "Test 3",
                Status = "Succeeded",
                Type = "CubeToCsv"
            });
            exports.Add(new FileExport()
            {
                Name = "Test 4",
                Status = "Succeeded",
                Type = "CubeToCsv"
            });
            group = new ExportCRONGroup()
            {
                FileExports = exports,
                GroupName = "Manual"
            };
            groups.Add(group);
            return groups;
        }

        public void UpdateExport(ExportUpdate update)
        {
            throw new NotImplementedException();
        }

        private IList<ExportCRONGroup> ConvertCubeToExportCRONGroup(ICube cube)
        {
            var exportCubes = new List<FetchExportCube>();
            foreach (var row in cube.GetRows())
            {
                var export = new FetchExportCube()
                {
                    Message = row["Message"]?.ToString(),
                    Name = row["Name"]?.ToString(),
                    IsDisabled = row["IsDisabled"] != null ? bool.Parse(row["IsDisabled"].ToString()) : false,
                    Type = row["Type"].ToString(),
                    ExportId = row["ExportId"] != null ? long.Parse(row["ExportId"].ToString()) : 0,
                    FileExportsHistoryId = row["FileExportsHistoryId"] != null ? long.Parse(row["FileExportsHistoryId"].ToString()) : 0,
                    IsForced = row["IsForced"] != null ? bool.Parse(row["IsForced"].ToString()) : false,
                    Schedule = row["Schedule"]?.ToString(),
                    TriggeredBy = row["TriggeredBy"]?.ToString(),
                };

                if (row["EndTime"] != null)
                    export.EndTime = DateTime.Parse(row["EndTime"].ToString());
                if (row["StartDateTime"] != null)
                    export.EndTime = DateTime.Parse(row["StartDateTime"].ToString());
                if (row["IsSuccessful"] != null)
                    export.IsSuccessful = bool.Parse(row["IsSuccessful"].ToString());

                exportCubes.Add(export);
            }

            return CreateGroupingForExports(exportCubes);
        }

        private IList<ExportCRONGroup> CreateGroupingForExports(List<FetchExportCube> exportCubes)
        {
            var groups = new List<ExportCRONGroup>();

            foreach (var exportCube in exportCubes.OrderBy(x => x.Schedule))
            {
                var group = groups.FirstOrDefault(x => x.Schedule == exportCube.Schedule);
                if (group == null)
                {
                    groups.Add(new ExportCRONGroup()
                    {
                        Schedule = exportCube.Schedule,
                        GroupName = string.IsNullOrEmpty(exportCube.Schedule) ? "Manual" : CronExpressionDescriptor.ExpressionDescriptor.GetDescription(exportCube.Schedule),
                        FileExports = new List<FileExport>()
                    });
                    group = groups.FirstOrDefault(x => x.Schedule == exportCube.Schedule);
                }

                group.FileExports.Add(new FileExport()
                {
                    FileExportsHistoryId = exportCube.FileExportsHistoryId,
                    ExportId = exportCube.ExportId,
                    Schedule = exportCube.Schedule,
                    Name = exportCube.Name,
                    Status = GetStatus(exportCube.FileExportsHistoryId, exportCube.IsSuccessful, exportCube.EndTime),
                    IsDisabled = exportCube.IsDisabled,
                    Type = exportCube.Type,
                    StartDateTime = exportCube.StartDateTime,
                    EndTime = exportCube.EndTime,
                    StartTime = exportCube.EndTime
                });
                ;
            }

            return groups;
        }

        private string GetStatus(long fileExportsHistoryId, bool? isSuccessful, DateTime? endTime)
        {
            if (fileExportsHistoryId == 0)
                return "Not Started";

            if (isSuccessful.HasValue)
                return isSuccessful.Value ? "Succeeded" : "Failed";

            return "Processing";
        }
    }
}