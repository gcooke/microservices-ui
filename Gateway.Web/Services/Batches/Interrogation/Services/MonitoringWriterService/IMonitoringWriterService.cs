using Gateway.Web.Services.Batches.Interrogation.Models.Enums;

namespace Gateway.Web.Services.Batches.Interrogation.Services.MonitoringWriterService
{
    public interface IMonitoringWriterService
    {
        void Write(MonitoringLevel monitoringLevel, string text);
        void WriteHeading(string text);
        void WriteText(string text);
        void WriteLine();
    }
}
