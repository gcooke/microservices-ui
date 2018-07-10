using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models.Monitoring;

namespace Gateway.Web.Services.Monitoring.RiskReports.RequestClients
{
    public class CubeRequestClient : BaseRequestClient
    {
        public CubeRequestClient(IGatewayRestService gateway, IRiskReportMonitoringCache reportMonitoringCache)
            : base(gateway, reportMonitoringCache)
        {
        }

        protected override RiskReportResponse ProcessResponse(RestResponse result, string query)
        {
            var payload = CubeBuilder.FromBytes(result.Content.Payload);

            return new RiskReportResponse
            {
                RowCount = payload.Rows,
                Query = query,
                CorrelationId = result.Content.CorrelationId.ToString(),
                TimingInMilliseconds = result.Content.TimeTakenMs,
                Successful = result.Successfull
            };
        }
    }
}