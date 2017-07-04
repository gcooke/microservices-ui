using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Gateway.Web.Models.AddIn
{
    [XmlType("AddInVersion")]
    public class AddInVersionModel
    {
        public long Id { get; set; }
        public long AddInId { get; set; }
        public string Name { get; set; }
    }
}