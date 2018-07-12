using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models.Monitoring;
using Newtonsoft.Json;

namespace Gateway.Web.Services.Monitoring.RiskReports
{
    public class RiskReportMonitoringService : IRiskReportMonitoringService
    {
        private readonly IGatewayRestService _gateway;

        public RiskReportMonitoringService(IGatewayRestService gateway)
        {
            _gateway = gateway;
        }

        public IEnumerable<RiskReportMetrics> GetMetricsForRiskReports(DateTime businessDate)
        {
            var previousBusiness = GetPreviousBusinessDay(businessDate);

            var currentMetrics = GetMetrics(businessDate);
            var previousMetrics = GetMetrics(previousBusiness);

            foreach (var currentMetric in currentMetrics)
            {
                var previousMetric = previousMetrics?.SingleOrDefault(x => x.Identifier == currentMetric.Identifier);
                currentMetric.PreviousResults = previousMetric?.Results;
            }

            return currentMetrics;
        }

        private List<RiskReportMetrics> GetMetrics(DateTime date)
        {
            var metrics = _gateway.Get("diagnostics", $"reports/monitoring/metrics/{date:yyyyMMdd}");

            if (!metrics.Successfull)
                return null;

            if (metrics.Content == null)
                return null;

            var payload = metrics.Content.GetPayloadAsString();
            return JsonConvert.DeserializeObject<List<RiskReportMetrics>>(payload);
        }

        private DateTime GetPreviousBusinessDay(DateTime businessDate)
        {
            var previousBusinessDay = businessDate.AddDays(-1);
            while (previousBusinessDay.DayOfWeek == DayOfWeek.Saturday ||
                   previousBusinessDay.DayOfWeek == DayOfWeek.Sunday)
            {
                previousBusinessDay = previousBusinessDay.AddDays(-1);
            }

            return previousBusinessDay;
        }
    }
}