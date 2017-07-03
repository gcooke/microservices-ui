using System.Collections.Generic;
using Gateway.Web.Models.Permission;

namespace Gateway.Web.Models.Group
{
    public class PermissionsModel
    {
        public PermissionsModel(long id)
        {
            Id = id;
            Items = new List<PermissionModel>();

        }

        public long Id { get; set; }
        public List<PermissionModel> Items { get; private set; }
    }
}