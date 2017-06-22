using System;
using System.Collections.Generic;
using System.Text;

namespace Gateway.Web.Utils
{
    public static class TimeSpanExtensions
    {
        private static Func<double, string, string> FormatValue = (number, description) =>  number == 0D ? string.Empty : string.Format("{0}{1}", number, description);

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
    }
}