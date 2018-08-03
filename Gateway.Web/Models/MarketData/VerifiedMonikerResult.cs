using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Models.MarketData
{
    public class VerifiedMonikersResult
    {
        public List<MonikerResult> Failures { get; set; }
        public List<MonikerResult> Successes { get; set; }
    }
}