using System.Xml.Serialization;

namespace Gateway.Web.Models.MarketData
{
    public class MonikerResult
    {
        [XmlAttribute]
        public bool Success { get; set; }
        [XmlAttribute]
        public string Moniker { get; set; }
    }

}