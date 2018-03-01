using System;
using System.Collections.Generic;

namespace Gateway.Web.Models.Request
{
    public class Summary
    {
        public Summary()
        {
            Items = new List<DetailRow>();
        }

        public string WallClockTime { get; set; }
        public List<DetailRow> Items { get; private set; }

        public Guid CorrelationId { get; set; }
        public Guid ParentCorrelationId { get; set; }
        public string User { get; set; }
        public string IpAddress { get; set; }
        public string Controller { get; set; }
        public string Version { get; set; }
        public string Resource { get; set; }
        public string RequestType { get; set; }
        public int Priority { get; set; }
        public bool IsAsync { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public int QueueTimeMs { get; set; }
        public int TimeTakenMs { get; set; }
        public int ResultCode { get; set; }
        public string ResultMessage { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Client { get; set; }

        public bool IsBusy
        {
            get { return EndUtc == DateTime.MinValue; }
        }

        public bool HasChildren
        {
            get { return Items.Count > 0; }
        }
    }

    public class DetailRow
    {
        public string Controller { get; set; }
        public int? RequestCount { get; set; }
        public int? SuccessfulCount { get; set; }
        public int? CompletedCount { get; set; }
        public string SizeUnit { get; set; }
        public int? Size { get; set; }
        public DateTime? MinStart { get; set; }
        public DateTime? MaxEnd { get; set; }
    }
}