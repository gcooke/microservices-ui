using System.Collections.Generic;
using Gateway.Web.Models.Group;

namespace Gateway.Web.Models.Security
{
    public class GroupsModel 
    {
        public GroupsModel()
        {
            Items = new List<GroupModel>();
        }

        public List<GroupModel> Items { get; private set; }
    }
}