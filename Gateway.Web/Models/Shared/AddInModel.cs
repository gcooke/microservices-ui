using System.Xml.Serialization;

namespace Gateway.Web.Models.Shared
{
    [XmlType("AddIn")]
    public class AddInModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public string Description { get; set; }
    }
}