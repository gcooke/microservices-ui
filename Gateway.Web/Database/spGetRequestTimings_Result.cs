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
    
    public partial class spGetRequestTimings_Result
    {
        public Nullable<System.Guid> CorrelationId { get; set; }
        public Nullable<System.Guid> ParentCorrelationId { get; set; }
        public string Controller { get; set; }
        public int QueueTimeMs { get; set; }
        public int TimeTakeMs { get; set; }
        public Nullable<int> Size { get; set; }
        public string SizeUnit { get; set; }
        public Nullable<System.DateTime> StartUtc { get; set; }
        public System.DateTime EndUtc { get; set; }
    }
}