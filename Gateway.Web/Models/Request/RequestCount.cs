using System;

namespace Gateway.Web.Models.Request
{
    public class RequestCount
    {
        public string Controller { get; set; }
        //public string Version { get; set; }
        public DateTime Hour { get; set; }
        public int Count { get; set; }
    }
}