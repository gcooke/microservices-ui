using System;
using System.Collections.Generic;

namespace Gateway.Web.Models.Request
{
    public class Details : Summary
    {
        public Details()
        {
            Items = new List<DetailRow>();
        }

        public string WallClockTime { get; set; }
        public List<DetailRow> Items { get; private set; }
    }

    public class DetailRow
    {
        public string Controller { get; set; }
        public int? RequestCount { get; set; }
        public string SizeUnit { get; set; }
        public int? Size { get; set; }
        public DateTime? MinStart { get; set; }
        public DateTime? MaxEnd { get; set; }
    }
}