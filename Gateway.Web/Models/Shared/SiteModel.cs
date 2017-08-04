using System.Xml.Serialization;

namespace Gateway.Web.Models.Shared
{
    [XmlType("Site")]
    public class SiteModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}