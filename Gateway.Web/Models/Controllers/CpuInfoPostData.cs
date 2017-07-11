namespace Gateway.Web.Models.Controllers
{
    public class CpuInfoPostData
    {
        public string MachineName { get; set; }

        public double Processor { get; set; }

        public ulong MemUsage { get; set; }

        public ulong TotalMemory { get; set; }
    }
}