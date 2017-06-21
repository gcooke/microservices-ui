using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;


namespace Gateway.Web.Models.Request
{
    [DataContract(Name = "Request", Namespace = "")]
    [XmlType("Request")]
    public class RequestPayload
    {
        [DataMember(Order = 1)]
        public Guid CorrelationId { get; set; }

        [DataMember(Order = 2)]
        public Guid ParentCorrelationId { get; set; }

        [DataMember(Order = 3)]
        public string Controller { get; set; }

        [DataMember(Order = 4)]
        public string StartUtc { get; set; }

        [DataMember(Order = 5)]
        public string EndUtc { get; set; }

        [DataMember(Order = 6)]
        public int? Size { get; set; }

        [DataMember(Order = 7)]
        public int QueueTimeMs { get; set; }

        [DataMember(Order = 8)]
        public int ProcessingTimeMs{ get; set; }

        [DataMember(Order = 9)]
        public int TotalTimeMs { get; set; }

        [DataMember(Order = 10)]
        public string SizeUnit { get; set; }

        [DataMember(Name = "ChildRequests", Order = 11)]
        public ChildRequests ChildRequests { get; set; }

        public int StartTimeMs { get; set; }

        public decimal QueueTime { get; set; }

        public decimal ProcessingTime { get; set; }

        public decimal StartTime { get; set; }
    }

    [CollectionDataContract(Name = "ChildRequests", ItemName = "Request", Namespace = "")]
    public class ChildRequests : List<RequestPayload>
    {
        public ChildRequests()
        {
        }

        public ChildRequests(IEnumerable<RequestPayload> source) : base(source)
        {
        }
    }
}