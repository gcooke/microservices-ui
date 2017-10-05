using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Database;

namespace Gateway.Web.Models.Controller
{
    public class LiveQueueChartModel
    {
        public IDictionary<string, List<int>> Data { get; set; }

        public LiveQueueChartModel(IEnumerable<QueueSizeModel> data)
        {
            var results = new Dictionary<string, List<int>>();
            Data = new Dictionary<string, List<int>>();

            foreach (var item in data)
            {
                List<int> values;
                if (results.TryGetValue(item.Label, out values))
                {
                    values.Add(item.Count);
                }
                else
                {
                    values = new List<int> { item.Count };
                    results.Add(item.Label, values);
                }
            }

            foreach (var item in results)
            {
                if (item.Value.Any(x => x > 0))
                {
                    Data.Add(item.Key, item.Value);
                }
            }
        }
    }

    public class QueueChartModel
    {
        public IDictionary<string, int[]> Data { get; set; }

        public QueueChartModel(IList<QueueSizeModel> data)
        {
            Data = new Dictionary<string, int[]>();

            var timeIntervals = new HashSet<DateTime>();
            var startDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);

            for (var i = 1; i <= 24; i++)
            {
                var value = startDateTime.AddHours(-1 * i);
                timeIntervals.Add(value);
            }

            foreach (var item in data.Select(x => x.Label).Distinct())
            {
                var values = new List<int>();
                foreach (var timeInterval in timeIntervals.OrderBy(x => x))
                {
                    values.Add(data.SingleOrDefault(x => x.Time == timeInterval && x.Label == item)?.Count ?? 0);
                }
                Data.Add(item, values.ToArray());
            }

        }
    }

}