using System.Web.Http;
using Gateway.Web.Hubs;
using Gateway.Web.Models.Controllers;
using Microsoft.AspNet.SignalR;

namespace Gateway.Web.Api
{
    public class CpuInfoController : ApiController
    {
        public void Post(CpuInfoPostData cpuInfo)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<CpuInfo>();
            context.Clients.All.cpuInfoMessage(cpuInfo.MachineName, cpuInfo.Processor, cpuInfo.MemUsage, cpuInfo.TotalMemory);
        }
    }
}