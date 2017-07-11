using System.Collections.Generic;
using System.Web.Mvc;
using Gateway.Web.Models.Permission;

namespace Gateway.Web.Models.Group
{
    public class PermissionsModel
    {
        public PermissionsModel(long id)
        {
            Id = id;
            Items = new List<PermissionModel>();
            AvailablePermissions = new List<SelectListItem>();
        }

        public long Id { get; set; }
        public List<PermissionModel> Items { get; private set; }
        public List<SelectListItem> AvailablePermissions { get; private set; }
    }
}