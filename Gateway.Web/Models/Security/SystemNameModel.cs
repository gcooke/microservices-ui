using System.Xml.Serialization;

namespace Gateway.Web.Models.Security
{
    [XmlType("SystemName")]
    public class SystemNameModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}