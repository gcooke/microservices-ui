namespace Gateway.Web.Models.Monitoring
{
    public class WorkerStats
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public int WorkerCount { get; set; }
        public int WorkerInProgressCount { get; set; }
    }
}