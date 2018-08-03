using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Gateway.Web.Models.Controller
{
    public class MissingMonikerModel : BaseControllerModel
    {
        public MissingMonikerModel(string name) : base(name)
        {
        }

        public List<MonikerResult> MissingMonikers { get; set; }

        public string RunDate { get; set; }
    }
    public class VerifiedMonikersResult
    {
        public List<MonikerResult> Failures { get; set; }
        public List<MonikerResult> Successes { get; set; }
    }

    public class MonikerResult
    {
        [XmlAttribute]
        public bool Success { get; set; }
        [XmlAttribute]
        public string Moniker { get; set; }
    }

    public class MarketDataResponse
    {
        public VerifiedMonikersResult VerifiedMonikersResult { get; set; }
        public bool Fallback { get; set; }
        public bool Successfull { get; set; }
        public string ErrorMessage { get; set; }
    }
}