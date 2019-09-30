using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Gateway.Web.Utils
{
    public static class StringEx
    {
        public static int ToIntOrDefault(this string input, int defaultValue = 0)
        {
            int result;
            if (int.TryParse(input, out result))
                return result;

            return defaultValue;
        }

        public static long ToLongOrDefault(this string input, long defaultValue = 0)
        {
            long result;
            if (long.TryParse(input, out result))
                return result;

            return defaultValue;
        }

        public static string MaxLength(this string input, int length)
        {
            var modifiedLength = length - 3;
            if (string.IsNullOrEmpty(input)) return input;
            if (input.Length <= modifiedLength) return input;
            return input.Substring(0, modifiedLength) + "...";
        }

        public static string AddWordspaces(this string input)
        {
            return new string(InsertSpacesBeforeCaps(input).ToArray()).Trim();
        }

        private static IEnumerable<char> InsertSpacesBeforeCaps(IEnumerable<char> input)
        {
            foreach (char c in input)
            {
                if (char.IsUpper(c))
                {
                    yield return ' ';
                }

                yield return c;
            }
        }

        public static string Truncate(this string value, int length, bool appendElipsis = true)
        {
            if (value.Length <= length)
                return value;

            value = value.Substring(0, length);

            if (appendElipsis)
                value += "...";

            return value;
        }

        public static decimal ToCultureAwareDecimal(this string stringValue)
        {
            if (string.IsNullOrWhiteSpace(stringValue))
                return 0;

            var culture = CultureInfo.CurrentCulture;
            var cultureDecimalSeparator = culture.NumberFormat.NumberDecimalSeparator;

            stringValue = stringValue.Replace(".", cultureDecimalSeparator);
            stringValue = stringValue.Replace(",", cultureDecimalSeparator);

            return decimal.Parse(stringValue, culture);
        }
    }
}