using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bagl.Cib.MIT.IoC;
using Gateway.Web.Models.Monitoring;
using Gateway.Web.Services.Monitoring.RiskReports.RequestClients;

namespace Gateway.Web.Services.Monitoring.RiskReports
{
    public class RiskReportMonitoringService : IRiskReportMonitoringService
    {
        private readonly ISystemInformation _systemInformation;
        private readonly IRiskReportMonitoringFileService _riskReportMonitoringFileService;
        private readonly IRiskReportMonitoringCache _riskReportMonitoringCache;

        public RiskReportMonitoringService(ISystemInformation systemInformation,
            IRiskReportMonitoringFileService riskReportMonitoringFileService,
            IRiskReportMonitoringCache riskReportMonitoringCache)
        {
            _systemInformation = systemInformation;
            _riskReportMonitoringFileService = riskReportMonitoringFileService;
            _riskReportMonitoringCache = riskReportMonitoringCache;
        }

        public IEnumerable<RiskReportMetrics> GetMetricsForRiskReports(DateTime businessDate, bool clearCache = false)
        {
            if (clearCache)
                _riskReportMonitoringCache.Clear();

            var previousBusinessDate = GetPreviousBusinessDay(businessDate);
            var riskReportMonitoringConfigItems = _riskReportMonitoringFileService.GetConfig();
            var tasks = new List<Task<RiskReportMetrics>>();
            var metrics = new List<RiskReportMetrics>();

            foreach (var config in riskReportMonitoringConfigItems)
            {
                var requestClient = GetRequestClient(config.RequestClient);

                if (requestClient == null)
                {
                    metrics.Add(new RiskReportMetrics
                    {
                        System = config.System,
                        ReportCategory = config.ReportCategory,
                        ReportSubCategory = config.ReportSubCategory,
                        ReportName = config.ReportName,
                        TMinus1 = null,
                        TMinus2 = null,
                        Parameters = GetParameters(config.Parameters)
                    });
                    continue;
                }

                var task = GetRiskReportMetricsTask(requestClient, config, businessDate, previousBusinessDate);
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            metrics.AddRange(tasks.Select(task => task.Result).ToList());

            return metrics;
        }

        private Task<RiskReportMetrics> GetRiskReportMetricsTask(IRequestClient requestClient, RiskReportMonitoringConfigItem config, DateTime businessDate, DateTime previousBusinessDate)
        {
            return Task.Factory.StartNew(() =>
            {
                var tMinus1Task = Task.Factory.StartNew(() => requestClient.GetDataCount(config, businessDate));
                var tMinus2Task = Task.Factory.StartNew(() => requestClient.GetDataCount(config, previousBusinessDate));

                Task.WaitAll(tMinus1Task, tMinus2Task);

                return new RiskReportMetrics
                {
                    System = config.System,
                    ReportCategory = config.ReportCategory,
                    ReportSubCategory = config.ReportSubCategory,
                    ReportName = config.ReportName,
                    TMinus1 = tMinus1Task.Result,
                    TMinus2 = tMinus2Task.Result,
                    Parameters = GetParameters(config.Parameters)
                };
            });
        }

        private IRequestClient GetRequestClient(string payloadEvaluator)
        {
            try
            {
                return _systemInformation.Resolve<IRequestClient>(payloadEvaluator);
            }
            catch (Exception ex)
            {
                return null;
            }
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

        private IDictionary<string, string> GetParameters(string parametersList)
        {
            var parameters = new Dictionary<string, string>();
            var paramPairs = parametersList.Split('|');

            if (paramPairs.Length == 0)
                return parameters;

            foreach (var paramPair in paramPairs)
            {
                var paramParts = paramPair.Split('=');

                if (paramParts.Length == 0)
                    continue;

                if (string.IsNullOrWhiteSpace(paramParts[0]))
                    continue;

                parameters[paramParts[0]] = paramParts[1];
            }

            return parameters;
        }
    }
}