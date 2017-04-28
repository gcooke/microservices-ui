using System.Collections.Generic;
using System.Xml.Serialization;

namespace Gateway.Web.Services
{
    public class ControllerInformation
    {
        public ControllerInformation()
        {
            Versions = new List<InformationServiceSet>();
        }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlArray]
        public List<InformationServiceSet> Versions { get; set; }
    }

    public class InformationServiceSet
    {
        public InformationNode Node { get; set; }
        public InformationService Service { get; set; }
        public InformationCheck[] Checks { get; set; }
    }

    public class InformationNode
    {
        public string Node { get; set; }
        public string Address { get; set; }
    }

    public class InformationService
    {
        public string ID { get; set; }
        public string Service { get; set; }
        public string[] Tags { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
    }

    public class InformationCheck
    {
        public string CheckID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Output { get; set; }
    }
}