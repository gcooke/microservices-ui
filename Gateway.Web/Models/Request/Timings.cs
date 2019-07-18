using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Utils;

namespace Gateway.Web.Models.Request
{
    public class Timings
    {
        private IEnumerable<RequestPayload> _flattenedPayloads;

        public Timings(RequestPayload root)
        {
            Root = root;
            Items = new List<RequestPayload> { root };
            CalculateTotals();
            CalculateSummaryTotals();
        }

        public RequestPayload Root { get; }

        public List<RequestPayload> Items { get; }

        public IEnumerable<ControllerSummary> ControllerSummaries { get; private set; }

        public decimal TotalTimeMs { get; private set; }

        public string TotalTime
        {
            get
            {
                return
                    TimeSpan.FromMilliseconds(
                        _flattenedPayloads.Where(t => !string.IsNullOrEmpty(t.EndUtc))
                            .DefaultIfEmpty(
                                new RequestPayload
                                {
                                    EndUtc = DateTime.MinValue.ToString(),
                                    StartUtc = DateTime.MinValue.ToString()
                                })
                            .Sum(t => (DateTime.Parse(t.EndUtc) - DateTime.Parse(t.StartUtc)).TotalMilliseconds)
                        )
                        .Humanize();
            }
        }

        public string WallClock { get; private set; }

        private void CalculateTotals()
        {
            // Calculate total time -  it is not necessarily the first request item (although it should be).
            _flattenedPayloads = FlattenChildHierarchy(Root).ToArray();

            var items = _flattenedPayloads
                        .Where(t => !string.IsNullOrEmpty(t.EndUtc))
                        .DefaultIfEmpty(new RequestPayload { EndUtc = DateTime.MinValue.ToString(), StartUtc = DateTime.MinValue.ToString() })
                        .ToList();

            var start = items.Min(t => DateTime.Parse(t.StartUtc));
            var end = items.Max(t => DateTime.Parse(t.EndUtc));

            var totalTime = (end - start);
            WallClock = totalTime.Humanize();
            TotalTimeMs = Math.Max(1, (decimal)totalTime.TotalMilliseconds);

            // Calculate start times offsets
            foreach (var payload in _flattenedPayloads)
            {
                var requestStart = DateTime.Parse(payload.StartUtc);
                payload.StartTimeMs = (int)(requestStart - start).TotalMilliseconds;
                payload.QueueTime = decimal.Round(decimal.Divide((payload.QueueTimeMs.GetValueOrDefault() * 100), TotalTimeMs));
                payload.ProcessingTime = decimal.Round(Math.Max(1,
                    decimal.Round(decimal.Divide((payload.ProcessingTimeMs.GetValueOrDefault() * 100), TotalTimeMs))));
                payload.StartTime = decimal.Round(decimal.Divide((payload.StartTimeMs * 100), TotalTimeMs));
            }
        }

        private void CalculateSummaryTotals()
        {
            ControllerSummaries = _flattenedPayloads
                .GroupBy(p => p.Controller)
                .Select(group =>
                {
                    var count = 0;
                    var totalQueueTime = 0;
                    var totalProcessingTime = 0;

                    foreach (var message in group)
                    {
                        ++count;
                        totalQueueTime += message.QueueTimeMs.GetValueOrDefault();
                        totalProcessingTime += message.ProcessingTimeMs.GetValueOrDefault();
                    }
                    
                    return new ControllerSummary()
                    {
                        Controller = group.Key,
                        CallCount = count,
                        TotalProcessingTime = TimeSpan.FromMilliseconds(totalProcessingTime),
                        TotalQueueTime = TimeSpan.FromMilliseconds(totalQueueTime)
                    };
                })
                .OrderBy(c => c.Controller)
                .ToList();
        }

        private IEnumerable<RequestPayload> FlattenChildHierarchy(RequestPayload payload)
        {
            yield return payload;

            if (payload.ChildRequests == null)
                yield break;

            foreach (var item in payload.ChildRequests)
            {
                foreach (var child in FlattenChildHierarchy(item))
                    yield return child;
            }
        }

        public class ControllerSummary
        {
            public string Controller { get; set; }

            public int CallCount { get; set; }

            public TimeSpan TotalQueueTime { get; set; }

            public TimeSpan TotalProcessingTime { get; set; }

            public string PrettyTotalQueueTime
                => TotalQueueTime.Humanize();

            public string PrettyTotalProcessingTime 
                => TotalProcessingTime.Humanize();
        }
    }
}
