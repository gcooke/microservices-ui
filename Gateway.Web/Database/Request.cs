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
    using System.Collections.Generic;
    
    public partial class Request
    {
        public long Id { get; set; }
        public System.Guid CorrelationId { get; set; }
        public System.Guid ParentCorrelationId { get; set; }
        public string User { get; set; }
        public string IpAddress { get; set; }
        public string Controller { get; set; }
        public string Version { get; set; }
        public string Resource { get; set; }
        public string RequestType { get; set; }
        public int Priority { get; set; }
        public bool IsAsync { get; set; }
        public System.DateTime StartUtc { get; set; }
        public System.DateTime UpdateTime { get; set; }
        public string ClientID { get; set; }
    }
}
