using System;
using Gateway.Web.Utils;

namespace Gateway.Web.Models.Controller
{
    public class HistoryItem
    {
        public Guid CorrelationId { get; set; }
        public long Id { get; set; }
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
        public int? ActualTimeTakenMs => TimeTakeMs - QueueTimeMs;
        public int? ResultCode { get; set; }
        public string ResultMessage { get; set; }
        public DateTime? RequestChangeUtc { get; set; }
        public string RequestChangeMessage { get; set; }

        public bool WasSuccessful
        {
            get { return ResultCode == 1; }
        }

        public int RelativePercentageQ { get; set; }
        public int RelativePercentageP { get; set; }

        public string QueueTimeFormatted
        {
            get
            {
                if (QueueTimeMs.HasValue)
                {
                    return (QueueTimeMs ?? 0).FormatTimeTaken();
                }
                return string.Empty;
            }
        }

        public string TimeTakenFormatted
        {
            get
            {
                if (TimeTakeMs.HasValue)
                {
                    // Show the actual time taken excluding the queue time.
                    return (ActualTimeTakenMs ?? 0).FormatTimeTaken();
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

        public string ControllerFormatted
        {
            get
            {
                return $"{Controller} ({Version})";
            }
        }

        public string UserDisplayName { get; set; }

        public bool IsSystemUser
        {
            get
            {
                if (User == null) return false;
                if (User.StartsWith("CORP\\svc", StringComparison.CurrentCultureIgnoreCase))
                    return true;
                if (User.StartsWith("INTRANET\\sys", StringComparison.CurrentCultureIgnoreCase))
                    return true;
                return false;
            }
        }

        public string UserFormatted
        {
            get
            {
                var name = UserDisplayName ?? User;
                if (string.IsNullOrEmpty(name) || !name.Contains("\\")) return name;
                return name.Substring(name.IndexOf('\\') + 1);
            }
        }
    }
}