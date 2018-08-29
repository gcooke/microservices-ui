using System.Xml.Serialization;

namespace Gateway.Web.Models.MarketData
{
    public class MonikerCheckResult
    {
        [XmlAttribute]
        public bool Success { get; set; }
        [XmlAttribute]
        public string Moniker { get; set; }
    }

}