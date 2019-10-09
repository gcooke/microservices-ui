using System.Collections.Generic;
using Gateway.Web.Models.Group;

namespace Gateway.Web.Models.User
{
    public class GroupsModel : IUserModel
    {
        public GroupsModel(long userId)
        {
            UserId = userId;
        }

        public GroupsModel() { }

        public long UserId { get; set; }

        public List<GroupModel> UserGroups { get; set; }

        public List<GroupModel> Groups { get; set; }

        public string Login { get; set; }

        public string FullName { get; set; }
    }
}