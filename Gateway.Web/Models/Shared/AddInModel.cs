using System.Collections.Generic;
using System.Xml.Serialization;
using Gateway.Web.Models.AddIn;

namespace Gateway.Web.Models.Shared
{
    [XmlType("AddIn")]
    public class AddInModel : ManifestModel
    {
        public List<AddInVersionModel> Version { get; set; }
        public override string Type { get { return "AddIn"; } }
    }
}