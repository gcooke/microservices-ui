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
        }

        public AddInsModel()
        {
        }

        public long Id { get; set; }

        public List<UserAddInVersionModel> ExcelAddInVersions { get; set; }

        public List<GroupAddInVersionModel> GroupExcelAddInVersions { get; set; }

        public List<SelectListItem> AvailableAddInVersions { get; set; }

        public string Login { get; set; }
    }
}