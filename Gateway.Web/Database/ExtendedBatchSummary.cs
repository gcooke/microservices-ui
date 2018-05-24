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
        public XElement Data { get; set; }
        public IDictionary<string, Tuple<int, int>> CalculationPricingRequestResults { get; set; }

        public ExtendedBatchSummary()
        {
            this.CalculationPricingRequestResults = new Dictionary<string, Tuple<int, int>>();
        }

        public int GetTradeCount()
        {
            var result = 0;
            foreach (var item in Data.Descendants("TradeSummary"))
            {
                var element = item.Element("Count");
                if (element != null)
                {
                    int value;
                    if (int.TryParse(element.Value, out value))
                        result += value;
                }
            }
            return result;
        }

        public int GetErrorCount()
        {
            var result = 0;
            foreach (var item in Data.Descendants("TradeSummary"))
            {
                var element = item.Element("Errors");
                if (element != null)
                {
                    int value;
                    if (int.TryParse(element.Value, out value))
                        result += value;
                }
            }
            return result;
        }
        public int GetMarginalTradeCount()
        {
            var result = 0;
            foreach (XElement item in Data.Descendants("TradeSummary"))
            {
                var element = item.Element("MarginalCount");
                if (element != null)
                {
                    int value;
                    if (int.TryParse(element.Value, out value))
                        result += value;
                }
            }
            return result;
        }

        public List<string> GetMarginalTrades()
        {
            List<string> result = new List<string>();
            // MarginalTrades
            foreach (XElement item in Data.Descendants("TradeSummary"))
            {
                XElement marginalTrades = item.Element("MarginalTrades");
                if (marginalTrades != null)
                {
                    foreach (XElement tradeId in marginalTrades.Descendants("string"))
                    {
                        result.Add(tradeId.Value);
                    }
                }
            }
            return result;
        }
    }
}