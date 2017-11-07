using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml.Serialization;
using Gateway.Web.Models.Group;

namespace Gateway.Web.Models.User
{
    [XmlType("UserAddIn")]
    public class AddInsModel : IUserModel
    {
        public AddInsModel(long id)
        {
            Id = id;

            AddIns = new List<UserAddInVersionModel>();
            AvailableAddInVersions = new List<SelectListItem>();
            GroupExcelAddInVersions = new List<GroupAddInVersionModel>();

            Applications = new List<UserApplicationVersionModel>();
            AvailableApplicationVersions = new List<SelectListItem>();
            GroupApplicationVersions = new List<GroupApplicationVersionModel>();
        }

        public long Id { get; private set; }

        public List<UserAddInVersionModel> AddIns { get; private set; }
        public List<SelectListItem> AvailableAddInVersions { get; private set; }

        public List<UserApplicationVersionModel> Applications { get; private set; }
        public List<SelectListItem> AvailableApplicationVersions { get; private set; }

        public List<GroupAddInVersionModel> GroupExcelAddInVersions { get; private set; }
        public List<GroupApplicationVersionModel> GroupApplicationVersions { get; private set; }

        public string Login { get; set; }
    }
}