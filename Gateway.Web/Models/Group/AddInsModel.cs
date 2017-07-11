using System.Collections.Generic;
using System.Web.Mvc;
using Gateway.Web.Models.AddIn;

namespace Gateway.Web.Models.Group
{
    public class AddInsModel
    {
        public AddInsModel(long id)
        {
            Id = id;
            Items = new List<GroupAddInVersionModel>();
            AvailableAddIns = new List<AddInModel>();
            AvailableVersions = new List<SelectListItem>();
        }

        public long Id { get; set; }
        public List<GroupAddInVersionModel> Items { get; private set; }
        public List<AddInModel> AvailableAddIns { get; private set; }
        public List<SelectListItem> AvailableVersions { get; private set; }
    }
}