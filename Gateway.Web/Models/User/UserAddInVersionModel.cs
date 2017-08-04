using System.Xml.Serialization;
using Gateway.Web.Models.Shared;

namespace Gateway.Web.Models.User
{
    [XmlType("UserAddInVersion")]
    public class UserAddInVersionModel : EntityAddInVersionModel
    {
        public long UserId { get; set; }
    }
}