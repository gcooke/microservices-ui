using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Request;

namespace Gateway.Web.Database
{
    public class HistoricalSummary
    {
        private readonly RequestCount[] _requests;

        public HistoricalSummary(RequestCount[] requests)
        {
            _requests = requests;
        }

        public QueueChartModel GetSummaryForAllControllers()
        {
            var controllers = GetMaxUsedControllers();
            var list = new Dictionary<string, QueueSizeModel>();

            foreach (var item in _requests)
            {
                var label = controllers.Contains(item.Controller.ToLower()) ? item.Controller : "Other";
                var key = label + item.Hour.ToString("ddMMM-HHmm");
                QueueSizeModel model;
                if (!list.TryGetValue(key, out model))
                {
                    model = new QueueSizeModel();
                    model.Label = label;
                    model.Time = item.Hour;
                    list.Add(key, model);
                }

                model.Count += item.Count;
            }
            return new QueueChartModel(list.Values);
        }

        public QueueChartModel GetSummaryForSelectedControllers(IList<string> controllers)
        {
            var list = new Dictionary<string, QueueSizeModel>();
            var lookup = controllers.Select(c => c.ToLower()).ToList();

            foreach (var item in _requests)
            {
                if (!lookup.Contains(item.Controller.ToLower())) continue;

                var key = item.Controller + item.Hour.ToString("ddMMM-HHmm");
                QueueSizeModel model;
                if (!list.TryGetValue(key, out model))
                {
                    model = new QueueSizeModel();
                    model.Label = item.Controller;
                    model.Time = item.Hour;
                    list.Add(key, model);
                }

                model.Count += item.Count;
            }
            return new QueueChartModel(list.Values);
        }
        
        public IDictionary<string, int> GetCurrentControllerQueueSize(IList<string> controllers)
        {
            throw new System.NotImplementedException();
        }

        internal IDictionary<string, int> GetCurrentControllerQueueSize()
        {
            throw new NotImplementedException();
        }

        private string[] GetMaxUsedControllers()
        {
            return _requests.GroupBy(r => r.Controller)
                .Select(g =>
                {
                    return new
                    {
                        Controller = g.Key,
                        Count = g.Sum(r => r.Count)
                    };
                })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .Select(g => g.Controller.ToLower())
                .ToArray();
        }

    }
}