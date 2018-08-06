using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Models.MarketData
{
    public class VerifiedMonikersResult
    {
        public List<MonikerCheckResult> Failures { get; set; }
        public List<MonikerCheckResult> Successes { get; set; }
    }
}