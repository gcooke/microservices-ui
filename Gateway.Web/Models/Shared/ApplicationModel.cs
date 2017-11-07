using System.Collections.Generic;
using System.Xml.Serialization;
using Gateway.Web.Models.AddIn;

namespace Gateway.Web.Models.Shared
{
    [XmlType("Application")]
    public class ApplicationModel : ManifestModel
    {
        public override string Type { get { return "Application"; } }

        public List<ApplicationVersionModel> Version { get; set; }
    }
}