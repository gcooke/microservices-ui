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
    
    public partial class ScheduledJob
    {
        public long Id { get; set; }
        public long ScheduleId { get; set; }
        public string JobId { get; set; }
        public System.DateTime BusinessDate { get; set; }
        public Nullable<System.Guid> RequestId { get; set; }
        public Nullable<System.DateTime> StartedAt { get; set; }
        public Nullable<System.DateTime> FinishedAt { get; set; }
        public string Status { get; set; }
    
        public virtual Schedule Schedule { get; set; }
    }
}