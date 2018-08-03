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
    
    public partial class RiskBatchConfiguration
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RiskBatchConfiguration()
        {
            this.RiskBatchSchedules = new HashSet<RiskBatchSchedule>();
        }
    
        public long ConfigurationId { get; set; }
        public string Type { get; set; }
        public string TradeSourceType { get; set; }
        public string MarketDataMapName { get; set; }
        public string OutputType { get; set; }
        public string OutputTag { get; set; }
        public int StateTtlMinutes { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RiskBatchSchedule> RiskBatchSchedules { get; set; }
    }
}