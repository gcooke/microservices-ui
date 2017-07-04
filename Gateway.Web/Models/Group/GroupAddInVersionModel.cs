using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Gateway.Web.Models.Group
{
    [XmlType("GroupAddInVersion")]
    public class GroupAddInVersionModel
    {
        public long GroupId { get; set; }
        public long ExcelAddInVersionId { get; set; }
        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public string Version { get; set; }
    }
}