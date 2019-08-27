using System;

namespace Gateway.Web.Models.Controller
{
    public class ServiceInfoModel
    {
        public string Id { get; set; }
        public string Server { get; set; }
        public string QueueName { get; set; }
        public WorkerStatus Status { get; set; }
        public DateTime LastUpdate { get; set; }
        public string InProgressId { get; set; }
        public int? PID { get; set; }
        public string ProcessName { get; set; }
        public string Hashkey { get; set; }
        public string ChannelKey { get; set; }
        public string Controller { get; set; }
        public string Version { get; set; }
        public int Timeout { get; set; }
        public DateTime Created { get; set; }
        public string Priority { get; set; }
    }

    public enum WorkerStatus
    {
        Idle,
        Starting,
        Processing,
        Shuttingdown,
        Shutdown,
        Parked,
        StartupError
    }
}