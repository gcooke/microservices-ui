using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Utils
{
    public static class SiteConstants
    {
        public static string DateFormat = "dd MMM yyyy";
        public static string DateParamFormat = "yyyy-MM-dd";

        public static bool ContainsSites(this string[] sites)
        {
            if (sites == null)
                return false;

            if (sites.Length == 1 && sites[0] == "NONE")
                return false;

            return sites.Length > 0;
        }
    }
}