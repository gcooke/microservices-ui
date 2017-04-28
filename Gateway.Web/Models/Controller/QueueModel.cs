using System;

namespace Gateway.Web.Models.Controller
{
    public class QueueModel
    {
        public string Server { get; set; }
        public string Controller { get; set; }
        public string Version { get; set; }
        public DateTime? LastEnqueue { get; set; }
        public DateTime? LastDequeue { get; set; }
        public int Length { get; set; }
        public int Workers { get; set; }
    }
}