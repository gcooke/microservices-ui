using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Gateway.Web.Database
{
    public class ExtendedBatchSummary
    {
        public string ControllerVersion { get; set; }
        public string Resource { get; set; }
        public bool Successfull { get; set; }
        public string Message { get; set; }
        public Guid CorrelationId { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public IDictionary<string, Tuple<int, int>> CalculationPricingRequestResults { get; set; }
        public string Name { get; set; }

        public long? Trades { get; set; }
        public long? PricingRequests { get; set; }
        public long? MarketDataRequests { get; set; }
        public long? RiskDataRequests { get; set; }

        public ExtendedBatchSummary()
        {
            this.CalculationPricingRequestResults = new Dictionary<string, Tuple<int, int>>();
        }
    }
}