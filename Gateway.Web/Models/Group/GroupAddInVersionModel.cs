using System.Xml.Serialization;
using Gateway.Web.Models.Shared;

namespace Gateway.Web.Models.Group
{
    [XmlType("GroupAddInVersion")]
    public class GroupAddInVersionModel : EntityAddInVersionModel, IGroupModel
    {
        public long GroupId { get; set; }
        public string Name { get; set; }
    }
}