using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Database;

namespace Gateway.Web.Models
{
    public class RecentRequests
    {
        private Dictionary<string, Dictionary<string, int>> _totals;

        public RecentRequests(IEnumerable<Request> req)
        {
            _totals = new Dictionary<string, Dictionary<string, int>>();
            var hour = DateTime.UtcNow.AddMinutes(-60);
            var minute = DateTime.UtcNow.AddSeconds(-60);

            foreach (var item in req.GroupBy(r => r.Controller))
            {
                var controller = item.Key.ToLower();
                var requests = item.Select(r => r.StartUtc).ToArray();
                var dict = new Dictionary<string, int>();
                dict.Add("24 hours", requests.Length);
                dict.Add("60 minutes", requests.Count(r => r >= hour));
                dict.Add("60 seconds", requests.Count(r => r >= minute));
                _totals.Add(controller, dict);
            }
        }

        public IEnumerable<KeyValuePair<string, int>> GetTotals(string controller)
        {
            var key = controller.ToLower();
            if (_totals.ContainsKey(key))
                return _totals[key];

            return new[] { new KeyValuePair<string, int>("24 hours", 0), };
        }
    }
}