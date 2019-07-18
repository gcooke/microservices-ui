using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Utils;

namespace Gateway.Web.Models.Request
{
    public class Timings
    {
        private IEnumerable<MessageHierarchy> _flattenedPayloads;

        public Timings(MessageHierarchy root)
        {
            Root = root;
            Items = new List<MessageHierarchy> { root };
            CalculateTotals();
            CalculateSummaryTotals();
        }

        public MessageHierarchy Root { get; }

        public List<MessageHierarchy> Items { get; }

        public IEnumerable<ControllerSummary> ControllerSummaries { get; private set; }

        public decimal TotalTimeMs { get; private set; }

        public string TotalTime
        {
            get
            {
                return
                    TimeSpan.FromMilliseconds(
                        _flattenedPayloads.Where(t => t.EndUtc != null)
                            .DefaultIfEmpty(
                                new MessageHierarchy
                                {
                                    EndUtc = DateTime.MinValue,
                                    StartUtc = DateTime.MinValue
                                })
                            .Sum(t => (t.EndUtc.GetValueOrDefault() - t.StartUtc).TotalMilliseconds)
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
                        .Where(t => t.EndUtc != null)
                        .DefaultIfEmpty(new MessageHierarchy { EndUtc = DateTime.MinValue, StartUtc = DateTime.MinValue })
                        .ToList();

            var start = items.Min(t => t.StartUtc);
            var end = items.Max(t => t.EndUtc.GetValueOrDefault());

            var totalTime = (end - start);
            WallClock = totalTime.Humanize();
            TotalTimeMs = Math.Max(1, (decimal)totalTime.TotalMilliseconds);

            // Calculate start times offsets
            foreach (var payload in _flattenedPayloads)
            {
                var requestStart = payload.StartUtc;
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
                    var totalPayloadSize = 0L;

                    foreach (var message in group)
                    {
                        ++count;
                        totalQueueTime += message.QueueTimeMs.GetValueOrDefault();
                        totalProcessingTime += message.ProcessingTimeMs.GetValueOrDefault();
                        totalPayloadSize += message.RequestSize + message.ResponseSize;
                    }
                    
                    return new ControllerSummary()
                    {
                        Controller = group.Key,
                        CallCount = count,
                        TotalProcessingTime = TimeSpan.FromMilliseconds(totalProcessingTime),
                        TotalQueueTime = TimeSpan.FromMilliseconds(totalQueueTime),
                        TotalPayloadSize = totalPayloadSize
                    };
                })
                .OrderBy(c => c.Controller)
                .ToList();
        }

        private IEnumerable<MessageHierarchy> FlattenChildHierarchy(MessageHierarchy payload)
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

            public long TotalPayloadSize { get; set; }

            public string PrettyTotalQueueTime
                => TotalQueueTime.Humanize();

            public string PrettyTotalProcessingTime 
                => TotalProcessingTime.Humanize();

            public string PrettyTotalPayloadSize
                => DataMeasurementUtils.SizeSuffix(TotalPayloadSize, 3);
        }
    }
}
