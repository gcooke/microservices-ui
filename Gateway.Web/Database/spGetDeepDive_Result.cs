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
    
    public partial class spGetDeepDive_Result
    {
        public Nullable<System.Guid> CorrelationId { get; set; }
        public string Controller { get; set; }
        public string Resource { get; set; }
        public Nullable<int> ResultCode { get; set; }
        public string ResultMessage { get; set; }
        public Nullable<System.Guid> ParentCorrelationId { get; set; }
        public byte[] Payload { get; set; }
        public Nullable<int> Depth { get; set; }
    }
}
