using System;
using Bagl.Cib.MIT.Cube;
using Gateway.Web.Models.Home;

namespace Gateway.Web.Database
{
    public class RiskBatchResult
    {
        public RiskBatchResult(string site, DateTime reportDate)
        {
            Site = site;
            BusinessDate = reportDate;
        }
        public string Site { get; set; }
        public string Name { get; set; }
        public DateTime? Started { get; set; }

        public string StartedFormated
        {
            get { return Started?.ToString("ddd HH:mm"); }
        }

        public DateTime? Completed { get; set; }
        public long? TimeTakenMs { get; set; }
        public string Duration { get; set; }
        public string State { get; set; }

        public StateItemState StateValue
        {
            get
            {
                StateItemState result;
                if (Enum.TryParse(State, true, out result))
                    return result;
                return StateItemState.Unknown;
            }
        }

        public bool IsLate
        {
            get { return Completed > SLA; }
        }

        public string StateMessage { get; set; }
        public string CorrelationId { get; set; }
        public DateTime? BusinessDate { get; set; }
        public DateTime? SLA { get; set; }

        public string SLAFormatted
        {
            get
            {
                return SLA?.ToString("ddd HH:mm ") ?? string.Empty;
            }
        }

        public bool? IsRerun { get; set; }
        public int? TotalRuns { get; set; }
        public long? Trades { get; set; }
        public long? PricingRequests { get; set; }
        public long? MarketDataRequests { get; set; }
        public long? RiskDataRequests { get; set; }
        public string Link { get; set; }

        public void Update(IRow row)
        {
            Site = row.GetStringValue("Site");
            Name = row.GetStringValue("Name");
            Started = row.GetValue<DateTime>("Started");
            Completed = row.GetValue<DateTime>("Completed");
            TimeTakenMs = row.GetValue<long>("TimeTakenMs");
            Duration = row.GetStringValue("Duration");
            State = row.GetStringValue("State");
            StateMessage = row.GetStringValue("StateMessage");
            CorrelationId = row.GetStringValue("CorrelationId");
            BusinessDate = row.GetValue<DateTime>("BusinessDate");
            SLA = row.GetValue<DateTime>("SLA");
            IsRerun = row.GetValue<bool>("IsRerun");
            TotalRuns = row.GetValue<int>("Runs");
            Trades = row.GetValue<long>("Trades");
            PricingRequests = row.GetValue<long>("PricingRequests");
            MarketDataRequests = row.GetValue<long>("MarketDataRequests");
            RiskDataRequests = row.GetValue<long>("RiskDataRequests");

            Link = "~/Request/Summary?correlationId=" + CorrelationId;
        }
    }
}