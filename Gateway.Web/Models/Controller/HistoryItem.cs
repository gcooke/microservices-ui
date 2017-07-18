using System;

namespace Gateway.Web.Models.Controller
{
    public class HistoryItem
    {
        private static TimeSpan Hour = TimeSpan.FromHours(1);
        private static TimeSpan Minute = TimeSpan.FromMinutes(1);
        private static TimeSpan Second = TimeSpan.FromSeconds(1);

        public Guid CorrelationId { get; set; }
        public string User { get; set; }
        public string IpAddress { get; set; }
        public string Controller { get; set; }
        public string Version { get; set; }
        public string Resource { get; set; }
        public string RequestType { get; set; }
        public int Priority { get; set; }
        public bool IsAsync { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime? EndUtc { get; set; }
        public int? QueueTimeMs { get; set; }
        public int? TimeTakeMs { get; set; }
        public int? ResultCode { get; set; }
        public string ResultMessage { get; set; }
        public DateTime? RequestChangeUtc { get; set; }
        public string RequestChangeMessage { get; set; }

        public bool WasSuccessful
        {
            get { return ResultCode == 1; }
        }

        public int RelativePercentage { get; set; }

        public string TimeTakenFormatted
        {
            get
            {
                if (TimeTakeMs.HasValue)
                {
                    var time = TimeSpan.FromMilliseconds(TimeTakeMs.Value);
                    if (time > Hour)
                        return string.Format("{0:N2} hours", time.TotalHours);

                    if (time > Minute)
                        return string.Format("{0:N2} mins", time.TotalMinutes);

                    if (time > Second)
                        return string.Format("{0:N2} secs", time.TotalSeconds);

                    return string.Format("{0} ms", TimeTakeMs);
                }
                return string.Empty;
            }
        }

        public bool IsComplete { get { return TimeTakeMs.HasValue; } }

        public string ResultFormatted
        {
            get
            {
                if (!TimeTakeMs.HasValue)
                    return RequestChangeMessage ?? "Pending...";

                if (ResultCode == 1)
                {
                    if (!string.IsNullOrEmpty(ResultMessage))
                        return ResultMessage;

                    return "Success";
                }
                else
                {
                    return "ERROR " + ResultMessage;
                }
            }
        }

        public string StartFormatted
        {
            get { return StartUtc.ToLocalTime().ToString("dd MMM HH:mm:ss"); }
        }

        public string UserFormatted
        {
            get
            {
                if (string.IsNullOrEmpty(User) || !User.Contains("\\")) return User;
                return User.Substring(User.IndexOf('\\') + 1);
            }
        }
    }
}