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
    
    public partial class QueueSize
    {
        public long Id { get; set; }
        public string Controller { get; set; }
        public string Version { get; set; }
        public System.DateTime UpdateTime { get; set; }
        public int ItemCount { get; set; }
        public System.DateTime LastEnqueue { get; set; }
        public Nullable<System.DateTime> LastDequeue { get; set; }
        public string Server { get; set; }
    }
}
