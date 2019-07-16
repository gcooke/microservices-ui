﻿using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Utils;

namespace Gateway.Web.Models.Request
{
    public class Timings
    {
        private IEnumerable<RequestPayload> _entireTree;

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
                        _entireTree.Where(t => !string.IsNullOrEmpty(t.EndUtc))
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
            _entireTree = GetAllChildren(Root).ToArray();

            var items = _entireTree
                        .Where(t => !string.IsNullOrEmpty(t.EndUtc))
                        .DefaultIfEmpty(new RequestPayload { EndUtc = DateTime.MinValue.ToString(), StartUtc = DateTime.MinValue.ToString() })
                        .ToList();

            var start = items.Min(t => DateTime.Parse(t.StartUtc));
            var end = items.Max(t => DateTime.Parse(t.EndUtc));

            var totalTime = (end - start);
            WallClock = totalTime.Humanize();
            TotalTimeMs = Math.Max(1, (decimal)totalTime.TotalMilliseconds);

            // Calculate start times offsets
            foreach (var payload in _entireTree)
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
            ControllerSummaries = _entireTree.GroupBy(p => p.Controller).Select(p =>
            {
                var totalTimeMs = p.Sum(x => x.ProcessingTimeMs + x.QueueTimeMs);
                var averageTimeMs = decimal.Divide(totalTimeMs.GetValueOrDefault(), Math.Max(1, p.Count()));

                return new ControllerSummary(p.Key, totalTimeMs.GetValueOrDefault(), (double)averageTimeMs);
            });
        }

        private IEnumerable<RequestPayload> GetAllChildren(RequestPayload payload)
        {
            yield return payload;

            if (payload.ChildRequests == null)
                yield break;

            foreach (var item in payload.ChildRequests)
            {
                foreach (var child in GetAllChildren(item))
                    yield return child;
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

            public string PrettyTotalTimeMs { get { return TimeSpan.FromMilliseconds(TotalTimeMs).Humanize(); } }

            public string PrettyAverageTimeMs
            {
                get { return TimeSpan.FromMilliseconds(AverageTimeMs).Humanize(); }
            }
        }
    }
}