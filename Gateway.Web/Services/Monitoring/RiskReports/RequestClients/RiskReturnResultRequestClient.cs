using System.Linq;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models.Monitoring;

namespace Gateway.Web.Services.Monitoring.RiskReports.RequestClients
{
    public class RiskReturnResultRequestClient : BaseRequestClient
    {
        public RiskReturnResultRequestClient(IGatewayRestService gateway, IRiskReportMonitoringCache reportMonitoringCache)
            : base(gateway, reportMonitoringCache)
        {
        }

        protected override RiskReportResponse ProcessResponse(RestResponse result, string query)
        {
            var payload = result.Content.GetPayloadAsXElement();

            return new RiskReportResponse
            {
                RowCount = payload?.Descendants("RiskReturnResult").Count(),
                Query = query,
                CorrelationId = result.Content.CorrelationId.ToString(),
                TimingInMilliseconds = result.Content.TimeTakenMs,
                Successful = result.Successfull
            };
        }
    }
}