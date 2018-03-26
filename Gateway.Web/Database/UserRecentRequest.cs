using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Gateway.Web.Database
{
    public class UserRecentRequest
    {
        public string User { get; set; }
        public DateTime Latest { get; set; }
        public int Total60Minutes { get; set; }
        public int Total24Hours { get; set; }
        public int Total7Days { get; set; }
        public string Groups { get; set; }
    }

    public class UserRecentRequests
    {
        private static TimeSpan Slot1 = TimeSpan.FromDays(7);
        private static TimeSpan Slot2 = TimeSpan.FromHours(24);
        private static TimeSpan Slot3 = TimeSpan.FromMinutes(120);

        private readonly Dictionary<string, UserRecentRequest> _history;

        public UserRecentRequests()
        {
            _history = new Dictionary<string, UserRecentRequest>();
        }

        public void Add(spGetUserReport_Result row)
        {
            UserRecentRequest target;
            if (!_history.TryGetValue(row.User, out target))
            {
                target = new UserRecentRequest() { User = row.User };
                _history.Add(target.User, target);
            }
            if (!row.Count.HasValue || !row.Date.HasValue) return;

            var actualDate = row.Date;
            var hour = TimeSpan.Parse(row.Hour);
            actualDate = actualDate.Value.Add(hour);
            var elapsed = DateTime.UtcNow.Subtract(actualDate.Value);

            if (elapsed < Slot3)
                target.Total60Minutes += row.Count.Value;
            else if (elapsed < Slot2)
                target.Total24Hours += row.Count.Value;
            else if (elapsed < Slot1)
                target.Total7Days += row.Count.Value;


            if (row.Latest.HasValue)
            {
                var latest = row.Latest.Value.ToLocalTime();
                if (latest > target.Latest)
                    target.Latest = latest;
            }

            target.Groups = row.Groups;
        }

        public IEnumerable<UserRecentRequest> GetAll()
        {
            return _history.Values.OrderByDescending(r => r.Latest);
        }
    }
}