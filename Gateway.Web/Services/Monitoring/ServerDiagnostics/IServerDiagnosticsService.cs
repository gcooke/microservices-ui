using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Web.Services.Monitoring.ServerDiagnostics
{
    public interface IServerDiagnosticsService
    {
        IDictionary<string, Models.Monitoring.ServerDiagnostics> Get();
    }
}
