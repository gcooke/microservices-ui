using System.Collections.Generic;
using System.Web.Mvc;

namespace Gateway.Web.Models.Group
{
    public class AddInsModel : IGroupModel
    {
        public AddInsModel(long id)
        {
            Id = id;
            AddIns = new List<GroupAddInVersionModel>();
            AvailableAddInVersions = new List<SelectListItem>();

            Applications = new List<GroupApplicationVersionModel>();
            AvailableApplicationVersions = new List<SelectListItem>();
        }

        public long Id { get; set; }
        public List<GroupAddInVersionModel> AddIns { get; private set; }
        public List<SelectListItem> AvailableAddInVersions { get; private set; }

        public List<GroupApplicationVersionModel> Applications { get; private set; }
        public List<SelectListItem> AvailableApplicationVersions { get; private set; }

        public string Name { get; set; }
    }
}