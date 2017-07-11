using System.Xml.Serialization;

namespace Gateway.Web.Models.Group
{
    [XmlType("Site")]
    public class SiteModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}