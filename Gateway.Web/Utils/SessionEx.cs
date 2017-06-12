using System;
using System.Web;

namespace Gateway.Web.Utils
{
    public static class SessionEx
    {
        public static void RegisterLastHistoryLocation(this HttpSessionStateBase session, Uri url)
        {
            session["history_url"] = url;
        }

        public static Uri GetLastHistoryLocation(this HttpSessionStateBase session)
        {
            var result = session["history_url"] as Uri;
            return result;
        }
    }
}