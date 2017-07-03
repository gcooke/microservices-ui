using System.Xml.Serialization;

namespace Gateway.Web.Models.Permission
{
    [XmlType("Permission")]
    public class PermissionModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long SystemNameId { get; set; }
        public string SystemName { get; set; }
    }
}