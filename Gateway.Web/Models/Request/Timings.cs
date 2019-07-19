using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Utils;

namespace Gateway.Web.Models.Request
{
    public class Timings
    {
        private IEnumerable<MessageHierarchy> _flattenedPayloads;
        private DateTime _start;
        private DateTime _end;

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

            _start = items.Min(t => t.StartUtc);
            _end = items.Max(t => t.EndUtc.GetValueOrDefault());

            var totalTime = _end - _start;
            WallClock = totalTime.Humanize();
            TotalTimeMs = Math.Max(1, (decimal)totalTime.TotalMilliseconds);

            // Calculate start times offsets
            foreach (var payload in _flattenedPayloads)
            {
                var requestStart = payload.StartUtc;
                payload.StartTimeMs = (int)(requestStart - _start).TotalMilliseconds;
                payload.QueueTime = decimal.Round(decimal.Divide((payload.QueueTimeMs.GetValueOrDefault() * 100), TotalTimeMs));
                payload.ProcessingTime = decimal.Round(Math.Max(1,
                    decimal.Round(decimal.Divide((payload.ProcessingTimeMs.GetValueOrDefault() * 100), TotalTimeMs))));
                payload.StartTime = decimal.Round(decimal.Divide((payload.StartTimeMs * 100), TotalTimeMs));
            }
        }

        private static string GetControllerLabel(string controller, IEnumerable<MessageHierarchy> messages)
        {
            if (!string.Equals(controller, "tradestore", StringComparison.CurrentCultureIgnoreCase))
                return controller;

            var details = messages
                .Where(m => string.Equals(m.SizeUnit, "Trades", StringComparison.InvariantCultureIgnoreCase))
                .Sum(t => t.Size.GetValueOrDefault());

            return $"{controller} ({details} Trades)";
        }

        private void CalculateSummaryTotals()
        {
            // populated in the local CreateSummary() function
            var serializedProcessingTime = 0L;

            var summaries = _flattenedPayloads
               .GroupBy(p => p.Controller)
               .Select(g => CreateSummary(g.Key, g.ToList()))
               .OrderBy(c => c.EarliestStartTime.TotalMilliseconds)
               .ToList();

            foreach (var summary in summaries)
            {
                // percentage is calculated relative to the total message's processing time, less the processing time 
                // for the parent message
                var time = string.Equals(summary.Controller, Root.Controller)
                    ? summary.TotalProcessingTime.TotalMilliseconds - Root.ProcessingTimeMs.GetValueOrDefault()
                    : summary.TotalProcessingTime.TotalMilliseconds;

                var percentage = serializedProcessingTime != 0
                    ? time / serializedProcessingTime * 100
                    : 0;

                summary.RelativePercentage = (int)Math.Round(percentage, 0, MidpointRounding.AwayFromZero);
            }

            ControllerSummaries = summaries;

            ControllerSummary CreateSummary(string controller, List<MessageHierarchy> messages)
            {
                var count = 0;
                var totalQueueTime = 0;
                var totalProcessingTime = 0;
                var totalPayloadSize = 0L;
                var startTime = DateTime.MaxValue;

                foreach (var message in messages)
                {
                    ++count;
                    totalQueueTime += message.QueueTimeMs.GetValueOrDefault();
                    totalProcessingTime += message.ProcessingTimeMs.GetValueOrDefault();
                    totalPayloadSize += message.RequestSize + message.ResponseSize;

                    if (message.CorrelationId != Root.CorrelationId)
                        serializedProcessingTime += message.ProcessingTimeMs.GetValueOrDefault();

                    if (message.StartUtc < startTime)
                        startTime = message.StartUtc;
                }

                return new ControllerSummary()
                {
                    Controller = controller,
                    SummaryText = GetControllerLabel(controller, messages),
                    CallCount = count,
                    TotalProcessingTime = TimeSpan.FromMilliseconds(totalProcessingTime),
                    TotalQueueTime = TimeSpan.FromMilliseconds(totalQueueTime),
                    EarliestStartTime = startTime - Root.StartUtc,
                    TotalPayloadSize = totalPayloadSize
                };
            }
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

            public string SummaryText { get; set; }

            public int CallCount { get; set; }

            public TimeSpan TotalQueueTime { get; set; }

            public TimeSpan TotalProcessingTime { get; set; }

            public TimeSpan EarliestStartTime { get; set; }

            public long TotalPayloadSize { get; set; }

            public int RelativePercentage { get; set; }

            public string PrettyTotalQueueTime
                => TotalQueueTime.Humanize();

            public string PrettyTotalProcessingTime
                => TotalProcessingTime.Humanize();

            public string PrettyEarliestStartTime
                => EarliestStartTime.Humanize();

            public string PrettyTotalPayloadSize
                => string.Concat((TotalPayloadSize / (1024f * 1024f)).ToString("#,##0.0##"), " MB");
        }
    }
}