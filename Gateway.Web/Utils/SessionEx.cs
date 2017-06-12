using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Utils
{
    public static class SessionEx
    {
        public static void RegisterLastHistoryLocation(this HttpSessionStateBase session, Uri url)
        {
            var history = GetHistoryRecord(session);
            history.Items.Add(url);
            while(history.Items.Count > 3)
                history.Items.RemoveAt(0);
        }

        public static Uri GetLastHistoryLocation(this HttpSessionStateBase session)
        {
            var history = GetHistoryRecord(session);
            if (history.Items.Count > 0)
            {
                var result = history.Items.Last();
                history.Items.Remove(result);
                return result;
            }
            return null;
        }

        private static HistoryRecord GetHistoryRecord(HttpSessionStateBase session)
        {
            var result = session["history_record"] as HistoryRecord;
            if (result == null)
            {
                result = new HistoryRecord();
                session["history_record"] = result;
            }
            return result;
        }
    }

    public class HistoryRecord
    {
        public HistoryRecord()
        {            
            Items = new List<Uri>();
        }

        public List<Uri> Items { get; private set; }
    }
}