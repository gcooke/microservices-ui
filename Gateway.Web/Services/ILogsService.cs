using Gateway.Web.Models.Request;

namespace Gateway.Web.Services
{
    public interface ILogsService
    {
        Logs GetLogs(string correlationId);
    }
}