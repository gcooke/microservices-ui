using System.Collections.Generic;
using Gateway.Web.Models.Request;

namespace Gateway.Web.Services
{
    public interface ILogsService
    {
        IEnumerable<string> GetRelevantControllerLogFileNames(LogsService.LogLocation location);
        Logs GetLogs(string correlationId);
    }
}