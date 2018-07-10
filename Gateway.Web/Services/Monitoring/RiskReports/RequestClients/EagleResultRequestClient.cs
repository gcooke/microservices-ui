using System.Linq;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models.Monitoring;

namespace Gateway.Web.Services.Monitoring.RiskReports.RequestClients
{
    public class EagleResultRequestClient : BaseRequestClient
    {
        public EagleResultRequestClient(IGatewayRestService gateway, IRiskReportMonitoringCache reportMonitoringCache)
            : base(gateway, reportMonitoringCache)
        {
        }

        protected override RiskReportResponse ProcessResponse(RestResponse result, string query)
        {
            var payload = result.Content.GetPayloadAsXElement();

            return new RiskReportResponse
            {
                RowCount = payload?.Descendants("PfeRecord").Count(),
                Query = query,
                CorrelationId = result.Content.CorrelationId.ToString(),
                TimingInMilliseconds = result.Content.TimeTakenMs,
                Successful = result.Successfull
            };
        }
    }
}