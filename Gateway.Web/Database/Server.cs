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
    
    public partial class Server
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Server()
        {
            this.Controllers = new HashSet<Controller>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
        public int RAM { get; set; }
        public int CpuCores { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Controller> Controllers { get; set; }
    }
}
