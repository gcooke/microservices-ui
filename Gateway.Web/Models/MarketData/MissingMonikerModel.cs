using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Gateway.Web.Models.Controller;

namespace Gateway.Web.Models.MarketData
{
    public class MissingMonikerModel : BaseControllerModel
    {
        public MissingMonikerModel(string name) : base(name)
        {
        }

        public List<MonikerCheckResult> MissingMonikers { get; set; }

        public string RunDate { get; set; }
    }
}