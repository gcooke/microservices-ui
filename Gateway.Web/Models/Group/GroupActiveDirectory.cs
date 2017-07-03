using System.Xml.Serialization;

namespace Gateway.Web.Models.Group
{
    [XmlType("GroupAD")]
    public class GroupActiveDirectory
    {
        public long Id { get; set; }
        public long GroupId { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
    }
}