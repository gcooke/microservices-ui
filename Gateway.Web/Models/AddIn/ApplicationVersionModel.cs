using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Gateway.Web.Models.Group;
using Gateway.Web.Models.User;

namespace Gateway.Web.Models.AddIn
{
    [XmlType("ApplicationVersion")]
    public class ApplicationVersionModel
    {
        private static System.Version UnknownVersion = new Version();

        public string Application { get; set; }
        public string Version { get; set; }
        public List<UserApplicationVersionModel> UserApplicationVersions { get; set; }
        public List<GroupApplicationVersionModel> GroupApplicationVersions { get; set; }

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