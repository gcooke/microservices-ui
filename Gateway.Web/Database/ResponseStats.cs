using System;
using System.Collections.Generic;
using System.Globalization;

namespace Gateway.Web.Database
{
    public class ResponseStats
    {
        private readonly Dictionary<string, ResponseStat> _stats;

        public ResponseStats(IEnumerable<spGetResponseStatsAll_Result> data)
        {
            _stats = new Dictionary<string, ResponseStat>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in data)
            {
                Add(item.Controller, item.ResultCode, item.Count, item.AvgResponseMs);
            }
        }

        private void Add(string controller, int? resultCode, int? count, int? avgResponseMs)
        {
            ResponseStat stat;
            if (!_stats.TryGetValue(controller, out stat))
            {
                stat = new ResponseStat();
                _stats.Add(controller, stat);
            }

            if (count.HasValue)
            {
                if (resultCode == 0)
                    stat.SuccessCount += count.Value;
                else
                    stat.FailureCount += count.Value;
            }

            if (!avgResponseMs.HasValue) return;

            stat.AvgResponseMs = stat.AvgResponseMs == 0
                ? avgResponseMs.Value
                : (stat.AvgResponseMs + avgResponseMs.Value) / 2;
        }

        public int GetTotalCalls(string name)
        {
            ResponseStat stat;
            if (_stats.TryGetValue(name, out stat))
                return stat.SuccessCount + stat.FailureCount;
            return 0;
        }

        public int GetTotalErrors(string name)
        {
            ResponseStat stat;
            if (_stats.TryGetValue(name, out stat))
                return stat.FailureCount;
            return 0;
        }

        public string GetAverageResponse(string name)
        {
            ResponseStat stat;
            if (_stats.TryGetValue(name, out stat))
                return stat.AvgResponseMs + "ms";
            return "";
        }
    }

    public class ResponseStat
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int AvgResponseMs { get; set; }
    }
}