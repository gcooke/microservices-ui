using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Gateway.Web.Models.Controller;

namespace Gateway.Web.Models.Controllers
{
    [Serializable, XmlRoot(ElementName = "Catalogue")]
    public class Catalogue
    {

        [XmlAttribute(AttributeName = "AsOf")]
        public string AsOf { get; set; }

        [XmlArray("Controllers")]
        [XmlArrayItem("Controller")]
        public List<ConfigurationModel> Controllers { get; set; }
    }
}