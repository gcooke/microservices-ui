using System.Xml.Serialization;

namespace Gateway.Web.Models.Group
{
    [XmlType("Portfolio")]
    public class PortfolioModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public long? ParentId { get; set; }
        public long SourceSystemId { get; set; }
    }
}