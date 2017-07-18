using System;
using System.Collections.Generic;

namespace Gateway.Web.Models.Request
{
    public class Transitions
    {
        public Transitions()
        {
            Items = new List<RequestChangeModel>();
        }

        public Guid CorrelationId { get; set; }
        public string Controller { get; set; }
        public string Version { get; set; }

        public List<RequestChangeModel> Items { get; private set; }
    }

    public class RequestChangeModel
    {
        public long Id { get; set; }

        public DateTime Time { get; set; }

        public string TimeFormatted
        {
            get { return Time.ToString("dd MMM HH:mm:ss.fff"); }
        }

        public string Message { get; set; }        
    }
}