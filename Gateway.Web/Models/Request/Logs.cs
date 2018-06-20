using System;
using System.Collections.Generic;

namespace Gateway.Web.Models.Request
{
    public class Logs
    {
        public Logs(string correlationId)
        {
            Items = new List<Log>();

            Guid actual;
            if (Guid.TryParse(correlationId, out actual))
                CorrelationId = actual;
        }

        public Guid CorrelationId { get; set; }
        public string Controller { get; set; }
        public string Version { get; set; }

        public List<Log> Items { get; private set; }
    }

    public class Log
    {
        public Log(string title)
        {
            Title = title;
        }

        public string Title { get; set; }
        public string Location { get; set; }
        public string Content { get; set; }
    }
}