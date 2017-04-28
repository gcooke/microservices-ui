using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Gateway.Web.Services
{
    [XmlType("Controller")]
    public class ControllerQueueInformation
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "Version")]
        public QueueVersionInformation[] Versions { get; set; } 
    }

    [XmlType("Version")]
    public class QueueVersionInformation
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "Queue")]
        public QueueInformation[] Queues { get; set; }
    }

    [XmlType("Queue")]
    public class QueueInformation
    {
        [XmlAttribute]
        public int Length { get; set; }

        [XmlAttribute]
        public int Workers { get; set; }

        [XmlAttribute]
        public string LastEnqueue { get; set; }

        [XmlAttribute]
        public string LastDequeue { get; set; }
    }

}