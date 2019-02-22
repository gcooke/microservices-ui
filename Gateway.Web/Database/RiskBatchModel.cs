using System;
using System.Collections.Generic;

namespace Gateway.Web.Database
{
    public class RiskBatchModel
    {
        public RiskBatchModel()
        {
            Items = new List<RiskBatchGroup>();
        }

        public DateTime Generated { get; set; }
        public DateTime BusinessDate { get; set; }
        public List<RiskBatchGroup> Items { get; private set; }
    }
}