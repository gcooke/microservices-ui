using System.Xml.Serialization;
using Gateway.Web.Models.Shared;

namespace Gateway.Web.Models.Group
{
    [XmlType("GroupApplicationVersion")]
    public class GroupApplicationVersionModel : EntityApplicationVersionModel, IGroupModel
    {
        public long GroupId { get; set; }
        public string GroupName { get; set; }
    }
}