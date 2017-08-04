using System.Xml.Serialization;
using Gateway.Web.Models.Shared;

namespace Gateway.Web.Models.Group
{
    [XmlType("GroupAddInVersion")]
    public class GroupAddInVersionModel : EntityAddInVersionModel
    {
        public long GroupId { get; set; }
    }
}