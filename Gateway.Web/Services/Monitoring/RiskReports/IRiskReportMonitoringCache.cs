using Bagl.Cib.MIT.IO;
using Gateway.Web.Models.Monitoring;

namespace Gateway.Web.Services.Monitoring.RiskReports
{
    public interface IRiskReportMonitoringCache
    {
        IExpiryingCache<RiskReportResponse> Cache { get; }
        void Clear();
    }
}
