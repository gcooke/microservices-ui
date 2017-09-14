using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Gateway.Web.Models.Group;
using Gateway.Web.Models.User;

namespace Gateway.Web.Models.AddIn
{
    [XmlType("AddInVersion")]
    public class AddInVersionModel
    {
        private static System.Version UnknownVersion = new Version();

        public string AddIn { get; set; }
        public string Version { get; set; }
        public List<UserAddInVersionModel> UserAddInVersions { get; set; }
        public List<GroupAddInVersionModel> GroupAddInVersions { get; set; }

        [XmlIgnore]
        public Version ActualVersion
        {
            get
            {
                System.Version result;
                if (System.Version.TryParse(Version, out result))
                    return result;

                return UnknownVersion;
            }
        }
    }
}