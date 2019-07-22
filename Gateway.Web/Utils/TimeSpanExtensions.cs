using System;
using System.Collections.Generic;
using System.Text;

namespace Gateway.Web.Utils
{
    public static class TimeSpanExtensions
    {
        private static readonly TimeSpan Hour = TimeSpan.FromHours(1);
        private static readonly TimeSpan Minute = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan Second = TimeSpan.FromSeconds(1);

        private static string FormatValue(int number, string description)
            => number == 0 ? string.Empty : $"{number}{description}";

        public static string Humanize(this TimeSpan date)
        {
            if (date == TimeSpan.Zero)
                return "0s";

            var values = new List<string>();
            values.Add(FormatValue(date.Days, "d"));
            values.Add(FormatValue(date.Hours, "h"));
            values.Add(FormatValue(date.Minutes, "m"));
            values.Add(FormatValue(date.Seconds, "s"));
            values.Add(FormatValue(date.Milliseconds, "ms"));

            values.RemoveAll(string.IsNullOrEmpty);

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
                    return $"{time.TotalHours:N2} hours ({milliseconds:N0}ms)";

                return $"{time.TotalHours:N2} hours";
            }

            if (time > Minute)
            {
                if (includeMs)
                    return $"{time.TotalMinutes:N2} mins ({milliseconds:N0}ms)";

                return $"{time.TotalMinutes:N2} mins";
            }

            if (time > Second)
            {
                if (includeMs)
                    return $"{time.TotalSeconds:N2} secs ({milliseconds:N0}ms)";

                return $"{time.TotalSeconds:N2} secs";
            }

            return $"{milliseconds} ms";
        }
    }
}
