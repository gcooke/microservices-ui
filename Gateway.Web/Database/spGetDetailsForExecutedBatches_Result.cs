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
    
    public partial class spGetDetailsForExecutedBatches_Result
    {
        public long ScheduleId { get; set; }
        public long RiskBatchScheduleId { get; set; }
        public Nullable<System.DateTime> StartedAt { get; set; }
        public string BatchType { get; set; }
        public string Status { get; set; }
        public Nullable<int> TradeCount { get; set; }
        public Nullable<int> RiskCount { get; set; }
        public System.Guid CorrelationId { get; set; }
    }
}
