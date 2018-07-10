using System;
using Bagl.Cib.MIT.IO;
using Bagl.Cib.MIT.IO.Impl;
using Gateway.Web.Models.Monitoring;

namespace Gateway.Web.Services.Monitoring.RiskReports
{
    public class RiskReportMonitoringCache : IRiskReportMonitoringCache
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        public IExpiryingCache<RiskReportResponse> Cache { get; private set; }

        public void Clear()
        {
            Cache = new ExpiryingCache<RiskReportResponse>(TimeSpan.FromHours(5), _dateTimeProvider);
        }

        public RiskReportMonitoringCache(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            Cache = new ExpiryingCache<RiskReportResponse>(TimeSpan.FromHours(5), _dateTimeProvider);
        }
    }
}