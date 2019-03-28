using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models.Home;
using Newtonsoft.Json;

namespace Gateway.Web.Services.Monitoring.ServerDiagnostics
{
    public class ServerDiagnosticsService : IServerDiagnosticsService
    {
        private readonly IGateway _gatewayRestService;

        public ServerDiagnosticsService(IGateway gatewayRestService)
        {
            _gatewayRestService = gatewayRestService;
        }

        public IDictionary<string, Models.Monitoring.ServerDiagnostics> Get()
        {
            var response = _gatewayRestService.Get<string>("diagnostics", "servers/monitoring/metrics").Result;

            if (!response.Successfull)
                return new Dictionary<string, Models.Monitoring.ServerDiagnostics>();

            if (response.Body == null)
                return new Dictionary<string, Models.Monitoring.ServerDiagnostics>();

            var payload = response.Body;
            return JsonConvert.DeserializeObject<Dictionary<string, Models.Monitoring.ServerDiagnostics>>(payload);
        }

        public async Task<IDictionary<string, Models.Monitoring.ServerDiagnostics>> GetAsync()
        {
            return await Task.Factory.StartNew(() =>
            { 
                return this.Get();
            }).ConfigureAwait(false);
        }

    }
}