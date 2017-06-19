using System.Collections.Generic;
using System.Web.Mvc;

namespace Gateway.Web.Models.Controller
{
    public class Version
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Status { get; set; }
        
        public System.Version SemVar { get; set; }

        public bool IsActive
        {
            get { return Status == "Active"; }
        }


        public Version(string name, string alias, string status)
        {
            Name = name;
            Alias = alias;
            Status = status;


            System.Version v;
            if (System.Version.TryParse(name, out v))
                SemVar = v;
            else
                SemVar = new System.Version();

            ApplicableStatuses = new List<SelectListItem>();
        }

        public List<SelectListItem> ApplicableStatuses { get; private set; }

        
    }
}