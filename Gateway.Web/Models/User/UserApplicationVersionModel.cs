using System.Xml.Serialization;
using Gateway.Web.Models.Shared;

namespace Gateway.Web.Models.User
{
    [XmlType("UserApplicationVersion")]
    public class UserApplicationVersionModel : EntityApplicationVersionModel
    {
        public long UserId { get; set; }
    }
}