using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Models.Reporting
{
    public class PfeModel
    {
        public PfeModel()
        {
            Items = new List<XvaResult>();
        }

        public List<XvaResult> Items { get; set; }
    }

    public class XvaResult
    {
        public XvaResult()
        {
            Items = new List<PfePoint>();
        }
        public string Site { get; set; }
        public string Counterparty { get; set; }
        public List<PfePoint> Items { get; set; }
    }

    public class PfePoint
    {
        public string Date { get; set; }
        public string Value { get; set; }
    }
}