using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Utils
{
    public static class StringEx
    {
        public static long ToLongOrDefault(this string input, long defaultValue = 0)
        {
            long result;
            if (long.TryParse(input, out result))
                return result;

            return defaultValue;
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
    }
}