using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml.Serialization;
using Gateway.Web.Models.Group;

namespace Gateway.Web.Models.User
{
    [XmlType("User")]
    public class UserModel : IUserModel
    {
        public UserModel(long id)
        {
            Id = id;
        }

        public UserModel() { }

        public long Id { get; set; }

        public string Domain { get; set; }

        public string FullName { get; set; }

        public string Login { get; set; }

        public List<GroupModel> UserGroups { get; set; }

        public List<SelectListItem> Items { get; set; }
    }
}