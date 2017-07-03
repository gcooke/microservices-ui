using System.Collections.Generic;

namespace Gateway.Web.Models.Group
{
    public class ADGroupsModel
    {
        public ADGroupsModel(long id)
        {
            Id = id;
            Items = new List<GroupActiveDirectory>();
        }

        public long Id { get; set; }
        public List<GroupActiveDirectory> Items { get; set; }
    }
}