using System.Xml.Serialization;

namespace Gateway.Web.Models.AddIn
{
    [XmlType("AddInVersion")]
    public class AddInVersionModel
    {
        public string AddIn { get; set; }
        public string Version { get; set; }
    }
}