using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Models.Home;
using Gateway.Web.Utils;

namespace Gateway.Web.Database
{
    public class RiskBatchResult : StateItem
    {
        public RiskBatchResult(string site, DateTime date)
        {
            Resource = site;
            Date = date;
            TargetCompletion = date.AddHours(31);
            if (TargetCompletion < DateTime.Now)
            {
                Text = "SLA Breached";
                State = StateItemState.Warn;
            }
            else
            {
                Text = "No results";
                State = StateItemState.Warn;
            }
        }

        public void Update(ExtendedBatchSummary row, string site)
        {
            CorrelationId = row.CorrelationId;
            Started = row.StartUtc.ToLocalTime();
            TimeTakenMs = (int)(row.EndUtc - row.StartUtc).TotalMilliseconds;
            Completed = Started.AddMilliseconds(TimeTakenMs);
            Resource = row.Resource;
            Version = row.ControllerVersion;
            Site = site;
            State = StateItemState.Okay;
            Link = "~/Request/Summary?correlationId=" + CorrelationId;

            if (row.Successfull)
            {
                Text = "Complete";
            }
            else
            {
                var totalRequests = row.CalculationPricingRequestResults.Values.Sum(v => v.Item2);
                var totalSuccess = row.CalculationPricingRequestResults.Values.Sum(v => v.Item1);
                Text = string.Format("{0} pass / {1}", totalSuccess, totalRequests);
                State = StateItemState.Error;
            }

            Name = row.Name.MaxLength(30);

            Trades = row.Trades;
            PricingRequests = row.PricingRequests;
            MarketDataRequests = row.MarketDataRequests;
            RiskDataRequests = row.RiskDataRequests;

            //Time = string.Format("{0:ddd HH:mm}-{1:ddd HH:mm}", Started, Completed);
            Time = string.Format("{0:ddd HH:mm}", Completed);
            Duration = string.Format("{0}", FormatTimeTaken());


            if (State != StateItemState.Okay) return;
            
            // Some additional rules that affect batch results
            if (Trades <= 0)
            {
                Text = "No Trades";
                State = StateItemState.Warn;
                return;
            }

            if (PricingRequests <= 0)
            {
                Text = "No Pricing Requests";
                State = StateItemState.Warn;
                return;
            }

            if (RiskDataRequests < PricingRequests)
            {
                Text = "Not enough risk data calls";
                State = StateItemState.Warn;
                return;
            }
        }
        

        public void UpdateErrors(ExtendedBatchSummary row, List<BatchSummary> errorData)
        {
            var batchName = $"{Site.ToUpper()} - {Name.ToUpper()}";
            var errors = errorData.Where(x => x.LegalEntity.ToUpper() == batchName).ToList();

            if (!errors.Any())
            {
                ErrorCount = 0;
                return;
            }

            if (errors.Count == 1)
            {
                ErrorCount = errors.First()?.TotalErrorCount;
                return;
            }

            var error = errors.FirstOrDefault(x => x.LegalEntity.ToUpper() == batchName);
            ErrorCount = error?.TotalErrorCount;
        }

        public Guid CorrelationId { get; private set; }

        public DateTime Date { get; private set; }

        public string Resource { get; private set; }

        public string BatchName => $"{Site}-{Name}";

        public string Version { get; private set; }

        public string Site { get; private set; }

        public DateTime TargetCompletion { get; private set; }

        public DateTime Started { get; private set; }

        public string StartedFormatted
        {
            get { return Started == DateTime.MinValue ? string.Empty : Started.ToString("ddd HH:mm"); }
        }

        public DateTime Completed { get; private set; }

        public string CompletedFormatted
        {
            get { return Completed == DateTime.MinValue ? string.Empty : Completed.ToString("ddd HH:mm"); }
        }

        public long TimeTakenMs { get; private set; }

        public bool IsRerun { get; set; }

        public string Duration { get; set; }

        public long? Trades { get; set; }
        public long? PricingRequests { get; set; }
        public long? MarketDataRequests { get; set; }
        public long? RiskDataRequests { get; set; }

        private string FormatTimeTaken()
        {
            return TimeTakenMs.FormatTimeTaken();
        }

        public int? ErrorCount { get; set; }
    }
}