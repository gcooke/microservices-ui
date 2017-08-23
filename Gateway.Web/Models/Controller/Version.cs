using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace Gateway.Web.Models.Controller
{
    [XmlRoot(ElementName= "Version", Namespace= "Gateway.Web.Models.Controller")]
    public class Version
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Status { get; set; }

        [XmlIgnore]
        public System.Version SemVar {
            get
            {


                System.Version v;
                if (System.Version.TryParse(Name, out v))
                    return v;
                else
                    return new System.Version();

            }
        }

        public bool IsActive
        {
            get { return Status == "Active"; }
        }

        public Version()
        {
                
        }

        public Version(string name, string alias, string status)
        {
            Name = name;
            Alias = alias;
            Status = status;


            //System.Version v;
            //if (System.Version.TryParse(name, out v))
            //    SemVar = v;
            //else
            //    SemVar = new System.Version();

            ApplicableStatuses = new List<SelectListItem>();
        }

        public List<SelectListItem> ApplicableStatuses { get; private set; }

        
    }
}