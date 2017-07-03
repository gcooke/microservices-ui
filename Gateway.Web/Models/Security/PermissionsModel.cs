using System.Collections.Generic;
using Gateway.Web.Models.Permission;

namespace Gateway.Web.Models.Security
{
    public class PermissionsModel
    {
        public PermissionsModel()
        {
            Items = new List<SystemPermissions>();
        }

        public List<SystemPermissions> Items { get; private set; }
    }
}