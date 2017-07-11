using System.Collections.Generic;
using System.Web.Mvc;
using Gateway.Web.Models.Permission;

namespace Gateway.Web.Models.Security
{
    public class PermissionsModel
    {
        public PermissionsModel()
        {
            Items = new List<SystemPermissions>();
            AvailableSystems = new List<SelectListItem>();
        }

        public List<SystemPermissions> Items { get; private set; }
        public List<SelectListItem> AvailableSystems { get; private set; }
    }
}