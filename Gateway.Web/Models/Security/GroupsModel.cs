using System.Collections.Generic;
using System.Linq;
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

        public IDictionary<string, List<GroupModel>> GroupsPerBusinessFunction
        {
            get { return Items.GroupBy(x => x.BusinessFunction ?? "Elevated Privileges").ToDictionary(x => x.Key, x => x.ToList()); }
        }
    }
}