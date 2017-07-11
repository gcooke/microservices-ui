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
    }
}