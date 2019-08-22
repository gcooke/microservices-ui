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
    
    public partial class RiskBatchSchedule
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RiskBatchSchedule()
        {
            this.Schedules = new HashSet<Schedule>();
        }
    
        public long RiskBatchScheduleId { get; set; }
        public string TradeSource { get; set; }
        public string Site { get; set; }
        public string FundingCurrency { get; set; }
        public string ReportingCurrency { get; set; }
        public string MarketDataMap { get; set; }
        public long RiskBatchConfigurationId { get; set; }
        public string TradeSourceType { get; set; }
        public string AdditionalProperties { get; set; }
        public bool IsLive { get; set; }
        public bool IsT0 { get; set; }
    
        public virtual RiskBatchConfiguration RiskBatchConfiguration { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Schedule> Schedules { get; set; }
    }
}
