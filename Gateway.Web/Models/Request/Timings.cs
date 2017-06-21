using System;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Models.Request
{
    public class Timings
    {
        public Timings(RequestPayload root)
        {
            Root = root;
            Items = new List<RequestPayload> { root };
            CalculateTotals();
        }

        public RequestPayload Root { get; private set; }
        public List<RequestPayload> Items { get; private set; }

        public decimal TotalTimeMs { get; private set; }

        private void CalculateTotals()
        {
            // Calculate total time -  it is not necessarily the first request item (although it should be).
            var entireTree = GetAllChildren(Root).ToArray();
            var start = entireTree.Where(t => !string.IsNullOrEmpty(t.StartUtc)).Min(t => DateTime.Parse(t.StartUtc));
            var end = entireTree.Where(t => !string.IsNullOrEmpty(t.EndUtc)).Max(t => DateTime.Parse(t.EndUtc));
            TotalTimeMs = (decimal)(end - start).TotalMilliseconds;

            // Calculate start times offsets
            foreach (var payload in entireTree)
            {
                var requestStart = DateTime.Parse(payload.StartUtc);
                payload.StartTimeMs = (int)(requestStart - start).TotalMilliseconds;
            }
        }

        private IEnumerable<RequestPayload> GetAllChildren(RequestPayload payload)
        {
            yield return payload;
            foreach (var item in payload.ChildRequests)
            {
                foreach (var child in GetAllChildren(item))
                {
                    yield return child;
                }
            }
        }
    }
}