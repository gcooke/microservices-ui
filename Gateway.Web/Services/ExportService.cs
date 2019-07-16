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
            //_gateway.SetGatewayUrlForService(ControllerName, "http://localhost:7000/");
        }

        public ExportSchedule CreateExport(ExportSchedule insert)
        {
            var query = $"Exports/Create/{insert.Type}";

            var dto = new CubeToCsvExportDto()
            {
                Schedule = insert.Schedule,
                Name = insert.Name,
                StartDateTime = insert.StartDateTime,
                DestinationInfo = insert.DestinationInfo,
                SourceInfo = insert.SourceInfo
            };

            var response = _gateway.Put<string, string>(ControllerName, query, dto.Serialize()).Result;

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

        public FileExport FetchExport(long id)
        {
            ICube cube = null;
            var query = $"Exports/Fetch/Id/{id}";
            var resultTask = _gateway.Get<ICube>(ControllerName, query).GetAwaiter().GetResult();
            if (resultTask.Successfull)
                cube = resultTask.Body;
            else
                throw new Exception(resultTask.Message);

            var fileExportCube = cube.GetRow(0);
            var fileExport = new FileExport();
            fileExport.Id = fileExportCube.GetValue<long>("Id").Value;
            fileExport.Name = fileExportCube.GetStringValue("Name");
            fileExport.Type = fileExportCube.GetStringValue("Type");
            fileExport.DestinationInformation = fileExportCube.GetStringValue("DestinationInformation");
            fileExport.SourceInformation = fileExportCube.GetStringValue("SourceInformation");
            fileExport.FailureEmailAddress = fileExportCube.GetStringValue("FailureEmailAddress");
            fileExport.IsDeleted = fileExportCube.GetValue<bool>("IsDeleted").Value;
            fileExport.IsDisabled = fileExportCube.GetValue<bool>("IsDisabled").Value;
            fileExport.Schedule = fileExportCube.GetStringValue("Schedule");
            fileExport.SuccessEmailAddress = fileExportCube.GetStringValue("SuccessEmailAddress");
            fileExport.StartDateTime = fileExportCube.GetValue<DateTime>("StartDateTime").Value;

            return fileExport;
        }

        public IList<ExportCRONGroup> FetchExports(DateTime date)
        {
            var query = $@"Exports/Fetch/{date.ToString("yyyy-MM-dd")}";
            var cube = FetchCube(query);

            IList<ExportCRONGroup> result = ConvertCubeToExportCRONGroup(cube);

            return result;
        }

        public ExportResponse RunExport(long id, DateTime time)
        {
            var query = $"Export/Run/{id}/{time.ToString("yyyy-MM-dd")}";
            var response = _gateway.Put<string, string>(ControllerName, query, string.Empty).Result;

            if (!response.Successfull)
                return new ExportResponse() { Message = response.Message, Successful = false };
            else
                return new ExportResponse() { Message = response.Message, Successful = false };
        }

        public ExportResponse RunScheduleExport(DateTime time)
        {
            var query = $"Export/Run/Scheduled/{time.ToString("yyyy-MM-dd")}";
            var response = _gateway.Put<string, string>(ControllerName, query, string.Empty).Result;

            if (!response.Successfull)
                return new ExportResponse() { Message = response.Message, Successful = false };
            else
                return new ExportResponse() { Message = response.Message, Successful = false };
        }

        public void UpdateExport(ExportSchedule update)
        {
            var query = $"Exports/Update/{update.Type}";

            var dto = new CubeToCsvExportDto()
            {
                Id = update.Id,
                Schedule = update.Schedule,
                Name = update.Name,
                StartDateTime = update.StartDateTime,
                DestinationInfo = update.DestinationInfo,
                SourceInfo = update.SourceInfo
            };

            var response = _gateway.Put<string, string>(ControllerName, query, dto.Serialize()).Result;

            if (!response.Successfull)
            {
                throw new Exception(response.Message);
            }
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
                        FileExports = new List<FileExportViewModel>()
                    });
                    group = groups.FirstOrDefault(x => x.Schedule == exportCube.Schedule);
                }

                group.FileExports.Add(new FileExportViewModel()
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