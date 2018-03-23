
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
    
public partial class Controller
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Controller()
    {

        this.Versions = new HashSet<Version>();

    }


    public long Id { get; set; }

    public string Name { get; set; }

    public string Type { get; set; }

    public int UserCallLimitPerSec { get; set; }

    public Nullable<int> TimeToLiveSec { get; set; }

    public string Configuration { get; set; }

    public string ScalingStrategy { get; set; }

    public int Weighting { get; set; }

    public int MaxInstances { get; set; }

    public Nullable<int> TimeoutMilliSec { get; set; }

    public string Description { get; set; }



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<Version> Versions { get; set; }

}

}
