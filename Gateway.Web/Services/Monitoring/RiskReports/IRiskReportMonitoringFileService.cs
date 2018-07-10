using System.Collections.Generic;
using Gateway.Web.Models;
using Gateway.Web.Models.Monitoring;

namespace Gateway.Web.Services.Monitoring.RiskReports
{
    public interface IRiskReportMonitoringFileService
    {
        IList<RiskReportMonitoringConfigItem> GetConfig();
    }
}