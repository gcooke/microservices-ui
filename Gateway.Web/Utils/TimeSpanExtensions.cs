using System;
using System.Collections.Generic;
using System.Text;

namespace Gateway.Web.Utils
{
    public static class TimeSpanExtensions
    {
        private static TimeSpan Hour = TimeSpan.FromHours(1);
        private static TimeSpan Minute = TimeSpan.FromMinutes(1);
        private static TimeSpan Second = TimeSpan.FromSeconds(1);

        private static Func<double, string, string> FormatValue = (number, description) => number == 0D ? string.Empty : string.Format("{0}{1}", number, description);

        public static string Humanize(this TimeSpan date)
        {
            var values = new List<string>();
            values.Add(FormatValue(date.Days, "d"));
            values.Add(FormatValue(date.Hours, "h"));
            values.Add(FormatValue(date.Minutes, "m"));
            values.Add(FormatValue(date.Seconds, "s"));
            values.Add(FormatValue(date.Milliseconds, "ms"));

            values.RemoveAll(v => string.IsNullOrEmpty(v));
            var builder = new StringBuilder();
            for (var i = 0; i < values.Count; i++)
            {
                if (i > 1)
                    break;

                if (builder.Length > 0)
                    builder.Append(" ");

                builder.Append(values[i]);
            }
            return builder.ToString();
        }

        public static string FormatTimeTaken(this int milliseconds, bool includeMs = false)
        {
            return FormatTimeTaken((long)milliseconds, includeMs);
        }

        public static string FormatTimeTaken(this long milliseconds, bool includeMs = false)
        {
            var time = TimeSpan.FromMilliseconds(milliseconds);
            if (time > Hour)
            {
                if (includeMs)
                    return string.Format("{0:N2} hours ({1:N0}ms)", time.TotalHours, milliseconds);
                return string.Format("{0:N2} hours", time.TotalHours);
            }

            if (time > Minute)
            {
                if (includeMs)
                    return string.Format("{0:N2} mins ({1:N0}ms)", time.TotalMinutes, milliseconds);
                return string.Format("{0:N2} mins", time.TotalMinutes);
            }

            if (time > Second)
            {
                if (includeMs)
                    return string.Format("{0:N2} secs ({1:N0}ms)", time.TotalSeconds, milliseconds);
                return string.Format("{0:N2} secs", time.TotalSeconds);
            }
            return string.Format("{0} ms", milliseconds);
        }
    }
}