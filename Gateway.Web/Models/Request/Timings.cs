using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Utils;

namespace Gateway.Web.Models.Request
{
    public class Timings
    {
        public Timings(RequestPayload root)
        {
            Root = root;
            Items = new List<RequestPayload> { root };
            CalculateTotals();
            CalculateSummaryTotals();
        }

        public RequestPayload Root { get; private set; }

        public List<RequestPayload> Items { get; private set; }
        public IEnumerable<RequestPayload> EntireTree { get; private set; }

        public IEnumerable<ControllerSummary> ControllerSummaries { get; private set; }

        public decimal TotalTimeMs { get; private set; }

        //Include Parent total?? Or is that the all the child requests summed?
        public string TotalTime { get { return TimeSpan.FromMilliseconds(EntireTree.Sum(t => (DateTime.Parse(t.EndUtc) - DateTime.Parse(t.StartUtc)).TotalMilliseconds)).Humanize(2); } }

        public string WallClock { get { return TimeSpan.FromMilliseconds((double)TotalTimeMs).Humanize(2); } }

        private void CalculateTotals()
        {
            // Calculate total time -  it is not necessarily the first request item (although it should be).
            EntireTree = GetAllChildren(Root).ToArray();
            var start = EntireTree.Where(t => !string.IsNullOrEmpty(t.StartUtc)).Min(t => DateTime.Parse(t.StartUtc));
            var end = EntireTree.Where(t => !string.IsNullOrEmpty(t.EndUtc)).Max(t => DateTime.Parse(t.EndUtc));
            TotalTimeMs = (decimal)(end - start).TotalMilliseconds;

            Root.StartTimeMs = (int)(DateTime.Parse(Root.StartUtc) - start).TotalMilliseconds;
            Root.QueueTime = decimal.Round(decimal.Divide((Root.QueueTimeMs * 100), TotalTimeMs));
            Root.ProcessingTime = Math.Max(1, decimal.Round(decimal.Divide((Root.ProcessingTimeMs * 100), TotalTimeMs)));
            Root.StartTime = decimal.Round(decimal.Divide((Root.StartTimeMs * 100), TotalTimeMs));

            // Calculate start times offsets
            foreach (var payload in EntireTree)
            {
                var requestStart = DateTime.Parse(payload.StartUtc);
                payload.StartTimeMs = (int)(requestStart - start).TotalMilliseconds;
                payload.QueueTime = decimal.Round(decimal.Divide((payload.QueueTimeMs * 100), TotalTimeMs));
                payload.ProcessingTime = Math.Max(1,
                    decimal.Round(decimal.Divide((payload.ProcessingTimeMs * 100), TotalTimeMs)));
                payload.StartTime = decimal.Round(decimal.Divide((payload.StartTimeMs * 100), TotalTimeMs));
            }
        }

        private void CalculateSummaryTotals()
        {
            ControllerSummaries = EntireTree.GroupBy(p => p.Controller).Select(p =>
            {
                var totalTimeMs = p.Sum(x => x.ProcessingTimeMs + x.QueueTimeMs);
                var averageTimeMs = decimal.Divide(totalTimeMs, Math.Max(1, p.Count())); //sum(x => x.Size.HasValue? Size.Value:0) or count??

                return new ControllerSummary(p.Key, totalTimeMs, (double)averageTimeMs);
            });
        }

        private IEnumerable<RequestPayload> GetAllChildren(RequestPayload payload)
        {
            yield return payload;
            if (payload.ChildRequests != null)
            {
                foreach (var item in payload.ChildRequests)
                {
                    foreach (var child in GetAllChildren(item))
                    {
                        yield return child;
                    }
                }
            }
        }

        public class ControllerSummary
        {
            public ControllerSummary(string controller, double totalTimeMs, double averageTimeMs)
            {
                Controller = controller;
                TotalTimeMs = totalTimeMs;
                AverageTimeMs = averageTimeMs;
            }

            public string Controller { get; private set; }

            public double TotalTimeMs { get; private set; }

            public double AverageTimeMs { get; private set; }

            public string PrettyTotalTimeMs { get { return TimeSpan.FromMilliseconds(TotalTimeMs).Humanize(2); } }

            public string PrettyAverageTimeMs
            {
                get { return TimeSpan.FromMilliseconds(AverageTimeMs).Humanize(2); }
            }
        }
    }
}