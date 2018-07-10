using System;
using Gateway.Web.Models;
using Gateway.Web.Models.Monitoring;

namespace Gateway.Web.Services.Monitoring.RiskReports.RequestClients
{
    public interface IRequestClient
    {
        RiskReportResponse GetDataCount(RiskReportMonitoringConfigItem config, DateTime businessDate);
    }
}
