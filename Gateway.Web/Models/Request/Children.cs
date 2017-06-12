using System;
using System.Collections.Generic;
using Gateway.Web.Models.Controller;

namespace Gateway.Web.Models.Request
{
    public class Children
    {
        public Children()
        {
            Requests = new List<HistoryItem>();
        }

        public Guid CorrelationId { get; set; }
        public List<HistoryItem> Requests { get; private set; }
    }    
}