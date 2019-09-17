using CronExpressionDescriptor;
using Gateway.Web.Utils;
using System;
using System.Collections.Generic;

namespace Gateway.Web.Models.Schedule.Output
{
    public class ScheduleTask
    {
        public DateTime? BusinessDate { get; set; }

        public long? GroupId { get; set; }

        public long ScheduleId { get; set; }

        public string ScheduleKey { get; set; }

        public long? ParentId { get; set; }

        public string Status { get; set; }

        public int Retries { get; set; }

        public Guid? RequestId { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? FinishedAt { get; set; }

        public string GroupName { get; set; }

        public string Parent { get; set; }

        public string Schedule { get; set; }

        public long ChildrenCount { get; set; }

        public IDictionary<long, string> Children { get; set; }

        public string Type { get; }

        public bool IsLive { get; set; }

        public bool IsT0 { get; set; }

        public bool HasChildren => ChildrenCount > 0;

        public int TimeTakenMs => (int)((FinishedAt ?? DateTime.MinValue) - (StartedAt ?? DateTime.MinValue)).TotalMilliseconds;

        public string TimeTakenFormatted => TimeTakenMs.FormatTimeTaken();

        public string FriendlyScheduleDescription => string.IsNullOrWhiteSpace(Schedule)
            ? string.Empty
            : CronExpressionDescriptor.ExpressionDescriptor.GetDescription(Schedule, new Options() { Use24HourTimeFormat = true });

        public string Name { get; set; }

        public string Key { get; set; }

        public string RequestUrl { get; set; }

        public string RequestSearchController
        {
            get
            {
                if (string.IsNullOrEmpty(RequestUrl)) return string.Empty;

                var start = RequestUrl.IndexOf("/api/", StringComparison.CurrentCultureIgnoreCase);
                if (start <= 0) return string.Empty;

                var url = RequestUrl.Substring(start + 5);
                var end = url.IndexOf("/");
                url = url.Substring(0, end);
                return url;
            }
        }

        public string RequestSearchString
        {
            get
            {
                if (string.IsNullOrEmpty(RequestUrl)) return string.Empty;

                var start = RequestUrl.IndexOf("/api/", StringComparison.CurrentCultureIgnoreCase);
                if (start <= 0) return string.Empty;

                var url = RequestUrl.Substring(start + 5);
                start = url.IndexOf("/");
                url = url.Substring(start + 1);
                start = url.IndexOf("/");
                url = url.Substring(start + 1);
                if (url.Contains("%"))
                {
                    start = url.IndexOf("%");
                    url = url.Substring(0, start);
                }
                return url;
            }
        }

        public bool IsEnabled { get; set; }

        public bool IsBatch { get; set; }
        public bool IsExe { get; set; }

        public ScheduleTask(string type)
        {
            Children = new Dictionary<long, string>();
            Type = type;
        }
    }
}