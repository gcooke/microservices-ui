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
    
    public partial class RiskType
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public double Bump { get; set; }
        public string BumpType { get; set; }
        public Nullable<bool> Active { get; set; }
        public Nullable<bool> IsMetric { get; set; }
        public Nullable<bool> IsRisk { get; set; }
        public string DisplayName { get; set; }
    }
}
