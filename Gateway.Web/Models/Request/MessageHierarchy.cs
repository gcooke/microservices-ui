using System;

namespace Gateway.Web.Models.Request
{
    public class MessageHierarchy
    {
        public Guid CorrelationId { get; set; }

        public Guid ParentCorrelationId { get; set; }

        public string Controller { get; set; }

        public string StartUtc { get; set; }

        public string EndUtc { get; set; }

        public string Successful { get; set; }

        public int? Size { get; set; }

        public int? QueueTimeMs { get; set; }

        public int? ProcessingTimeMs{ get; set; }

        public int? TotalTimeMs { get; set; }

        public string SizeUnit { get; set; }

        public MessageHierarchy[] ChildRequests { get; set; }

        public int StartTimeMs { get; set; }

        public decimal QueueTime { get; set; }

        public decimal ProcessingTime { get; set; }

        public decimal StartTime { get; set; }
    }
}