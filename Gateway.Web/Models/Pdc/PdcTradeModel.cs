using System;

namespace Gateway.Web.Models.Pdc
{
    public class PdcTradeModel
    {
        public string BookingSystem { get; set; }
        public string SiteName { get; set; }
        public string SdsId { get; set; }
        public string Counterparty { get; set; }
        public string RequestId { get; set; }
        public string TradeId { get; set; }
        public string VersionId { get; set; }
        public DateTime RequestDate { get; set; }
        public string Instrument { get; set; }
        public bool PredealCheckResult { get; set; }
        public string PredealCheckReason { get; set; }
        public DateTime EntryDate { get; set; }
    }
}