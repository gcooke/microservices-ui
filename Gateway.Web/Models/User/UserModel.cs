using System.Xml.Serialization;

namespace Gateway.Web.Models.User
{
    [XmlType("User")]
    public class UserModel : IUserModel
    {
        public long Id { get; set; }
        public string Domain { get; set; }
        public string FullName { get; set; }
        public string Login { get; set; }
    }
}