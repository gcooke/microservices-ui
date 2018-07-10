using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models;
using Gateway.Web.Models.Monitoring;

namespace Gateway.Web.Services.Monitoring.RiskReports.RequestClients
{
    public abstract class BaseRequestClient : IRequestClient
    {
        protected IGatewayRestService GatewayRestService;
        private readonly IRiskReportMonitoringCache _riskReportMonitoringCache;

        protected BaseRequestClient(IGatewayRestService gatewayRestService, IRiskReportMonitoringCache riskReportMonitoringCache)
        {
            GatewayRestService = gatewayRestService;
            _riskReportMonitoringCache = riskReportMonitoringCache;
        }

        public RiskReportResponse GetDataCount(RiskReportMonitoringConfigItem config, DateTime businessDate)
        {
            var query = GetQuery(config, businessDate);
            var cancellationTokenSource = new CancellationTokenSource();

            RiskReportResponse riskReportResponse;
            if (TryRetrieveFromCache(query, out riskReportResponse))
            {
                return riskReportResponse;
            }

            cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(2));
            var result = GatewayRestService.Get(config.Controller, query, cancellationTokenSource.Token);

            if (result.Successfull == false)
            {
                riskReportResponse = new RiskReportResponse
                {
                    Successful = false,
                    Query = query,
                    CorrelationId = result.Content?.CorrelationId.ToString(),
                    RowCount = null,
                    TimingInMilliseconds = result.Content?.TimeTakenMs ?? 0
                };
            }
            else
            {
                riskReportResponse = ProcessResponse(result, query);
            }

            AddToCache(query, riskReportResponse);
            return riskReportResponse;
        }

        protected abstract RiskReportResponse ProcessResponse(RestResponse result, string query);

        protected long? GetDataSetId(DateTime date, string namePrefix)
        {
            var query = $"dataset/{namePrefix}/{date:yyyyMMdd}";
            var result = GatewayRestService.Get("riskdata", "official", query);

            if (!result.Successfull)
                return null;

            var payload = result.Content?.GetPayloadAsXElement();

            if (payload == null || !payload.Descendants("Id").Any())
                return null;

            return long.Parse(payload.Descendants("Id").First().Value);
        }

        protected string GetQuery(RiskReportMonitoringConfigItem config, DateTime businessDate)
        {
            if (!config.ResolveDataSet)
            {
                return config.Endpoint.Replace(config.DateParameter, businessDate.ToString(config.DateFormat));
            }

            var datasetId = GetDataSetId(businessDate, config.DataSetNamePrefix) ?? 0;
            return config.Endpoint.Replace(config.DataSetParameter, $"{datasetId}");
        }

        protected bool TryRetrieveFromCache(string query, out RiskReportResponse response)
        {
            if (!_riskReportMonitoringCache.Cache.ContainsKey(query))
            {
                response = null;
                return false;
            }

            if (!_riskReportMonitoringCache.Cache.TryGetValue(query, out response))
            {
                response = null;
                return false;
            }

            return true;
        }

        protected void AddToCache(string query, RiskReportResponse response)
        {
            _riskReportMonitoringCache.Cache.Add(query, response);
        }
    }
}