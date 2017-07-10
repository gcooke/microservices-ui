using Microsoft.AspNet.SignalR;

namespace Gateway.Web.Hubs
{
    public class CpuInfo : Hub
    {
        public void SendCpuInfo(string machineName, double processor, int memUsage, int totalMemory)
        {
            Clients.All.cpuInfoMessage(machineName, processor, memUsage, totalMemory);
        }
    }
}