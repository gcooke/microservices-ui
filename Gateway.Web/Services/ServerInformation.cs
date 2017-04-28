using System.Xml.Serialization;

namespace Gateway.Web.Services
{
    [XmlType("Server")]
    public class ServerInformation
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int Queues { get; set; }

        [XmlAttribute]
        public int Workers { get; set; }

        [XmlAttribute]
        public double Cpu { get; set; }

        [XmlAttribute]
        public double Memory { get; set; }

        [XmlAttribute]
        public string Disk { get; set; }        
    }
}