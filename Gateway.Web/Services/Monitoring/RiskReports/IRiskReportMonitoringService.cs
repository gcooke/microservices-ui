using System;
using System.Collections.Generic;
using Gateway.Web.Models.Monitoring;

namespace Gateway.Web.Services.Monitoring.RiskReports
{
    public interface IRiskReportMonitoringService
    {
        IEnumerable<RiskReportMetrics> GetMetricsForRiskReports(DateTime businessDate, bool clearCache = false);
    }
}