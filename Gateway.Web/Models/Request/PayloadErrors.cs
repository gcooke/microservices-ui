using System;
using System.Collections.Generic;

namespace Gateway.Web.Models.Request
{
    public class PayloadErrors
    {
        public List<ErrorRow> ErrorRows { get; set; }
        public Guid CorrelationId { get; set; }
        public string Controller { get; set; }
    }
}