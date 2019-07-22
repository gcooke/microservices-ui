using System;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Models.Pdc
{
    public class PdcTradesModel
    {
        public PdcTradesModel()
        {
            Items = new List<PdcTradeModel>();
            BusinessDate = DateTime.Today;
        }
        public DateTime BusinessDate { get;   set; }

        public List<PdcTradeModel> Items { get; private set; }
    }
}