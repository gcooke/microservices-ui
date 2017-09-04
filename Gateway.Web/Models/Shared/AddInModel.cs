using System.Collections.Generic;
using System.Xml.Serialization;
using Gateway.Web.Models.AddIn;

namespace Gateway.Web.Models.Shared
{
    [XmlType("AddIn")]
    public class AddInModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public string Description { get; set; }
        public List<AddInVersionModel> Version { get; set; }
    }
}