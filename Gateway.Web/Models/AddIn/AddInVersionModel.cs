using System.Collections.Generic;
using System.Xml.Serialization;
using Gateway.Web.Models.Group;
using Gateway.Web.Models.User;

namespace Gateway.Web.Models.AddIn
{
    [XmlType("AddInVersion")]
    public class AddInVersionModel
    {
        public string AddIn { get; set; }
        public string Version { get; set; }
        public List<UserAddInVersionModel> UserAddInVersions { get; set; }
        public List<GroupAddInVersionModel> GroupAddInVersions { get; set; }
    }
}