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
    
    public partial class spGetBatchSummary_Result
    {
        public long ScheduleId { get; set; }
        public System.DateTime BusinessDate { get; set; }
        public Nullable<System.Guid> RequestId { get; set; }
        public Nullable<System.DateTime> StartedAt { get; set; }
        public Nullable<System.DateTime> EndUtc { get; set; }
        public string BatchName { get; set; }
        public string TradeSource { get; set; }
        public string TradeSourceType { get; set; }
        public string Site { get; set; }
        public string MarketDataMap { get; set; }
        public Nullable<int> ResultCode { get; set; }
        public Nullable<int> TimeTakenMs { get; set; }
    }
}
