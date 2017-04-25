using System;

namespace Gateway.Web.Models.Controller
{
    public class HistoryItem
    {
        public Guid CorrelationId { get; set; }
        public string User { get; set; }
        public string IpAddress { get; set; }
        public string Controller { get; set; }
        public string Version { get; set; }
        public string Resource { get; set; }
        public string RequestType { get; set; }
        public int Priority { get; set; }
        public bool IsAsync { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime? EndUtc { get; set; }
        public int? QueueTimeMs { get; set; }
        public int? TimeTakeMs { get; set; }
        public int? ResultCode { get; set; }
        public string  ResultMessage { get; set; }
    }
}