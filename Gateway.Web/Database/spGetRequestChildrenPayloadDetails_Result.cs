//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gateway.Web.Database
{
    using System;
    
    public partial class spGetRequestChildrenPayloadDetails_Result
    {
        public System.Guid CorrelationId { get; set; }
        public System.Guid ParentCorrelationId { get; set; }
        public string Controller { get; set; }
        public string Version { get; set; }
        public System.DateTime StartUtc { get; set; }
        public Nullable<System.DateTime> EndUtc { get; set; }
        public Nullable<int> ResultCode { get; set; }
        public Nullable<int> QueueTimeMs { get; set; }
        public Nullable<int> TimeTakeMs { get; set; }
        public Nullable<int> Size { get; set; }
        public string SizeUnit { get; set; }
        public long RequestLength { get; set; }
        public long ResponseLength { get; set; }
        public string Resource { get; set; }
    }
}
