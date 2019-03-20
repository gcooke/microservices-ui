using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Gateway.Web.Models.Security
{
    [XmlType("Sigma")]
    public class Manifest
    {
        public Manifest()
        {
            Servers = new List<Server>();
            Controllers = new List<Controller>();
            Software = new Software();
        }

        public List<Server> Servers { get; set; }
        public Software Software { get; set; }
        public List<Controller> Controllers { get; set; }
    }

    [XmlType("Server")]
    public class Server
    {
        public Server()
        {
            WindowsServices = new List<WindowsService>();
            Websites = new List<Website>();
        }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Environment { get; set; }

        public List<WindowsService> WindowsServices { get; set; }
        public List<Website> Websites { get; set; }
    }

    [XmlType("service")]
    public class WindowsService
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string SharedFolderName { get; set; }
        [XmlAttribute]
        public string Executable { get; set; }

        [XmlElement("update")]
        public bool? Update { get; set; }
        [XmlElement("Delete")]
        public bool? Delete { get; set; }
        [XmlElement("version")]
        public string Version { get; set; }
    }

    [XmlType("website")]
    public class Website
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string SharedFolderName { get; set; }

        [XmlElement("update")]
        public bool? Update { get; set; }
        [XmlElement("version")]
        public string Version { get; set; }
    }

    [XmlType("Software")]
    public class Software
    {
        public Software()
        {
            WindowsServices = new List<WindowsService>();
            Websites = new List<Website>();
            SinglePageApplication = new List<Website>();
        }

        public string RootFileShare { get; set; }
        public List<WindowsService> WindowsServices { get; set; }
        public List<Website> Websites { get; set; }
        public List<Website> SinglePageApplication { get; set; }
    }

    [XmlType("Controller")]
    public class Controller
    {
        public Controller()
        {
            versions = new List<Version>();
        }
        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement("defaultstatus")]
        public string DefaultStatus { get; set; }

        public List<Version> versions { get; set; }
    }

    [XmlType("version")]
    public class Version
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("alias")]
        public string Alias { get; set; }
    }
}