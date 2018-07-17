namespace Gateway.Web.Models.Monitoring
{
    public class RequestStats
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public int RequestCount { get; set; }
        public int RequestInProgressCount { get; set; }
    }
}