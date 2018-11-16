namespace Gateway.Web.Services.Monitoring.ServerDiagnostics
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    
    public interface IServerDiagnosticsService
    {
        IDictionary<string, Models.Monitoring.ServerDiagnostics> Get();

        Task<IDictionary<string, Models.Monitoring.ServerDiagnostics>> GetAsync();
    }
}
