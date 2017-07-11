using System.Collections.Generic;
using Gateway.Web.Models.Security;

namespace Gateway.Web.Models.Permission
{
    public class SystemPermissions
    {
        public SystemPermissions(string name)
        {
            Name = name;
            Items = new List<PermissionModel>();
        }

        public string Name { get; set; }
        public List<PermissionModel> Items { get; private set; }
    }
}