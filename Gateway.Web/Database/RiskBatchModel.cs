using System.Collections.Generic;

namespace Gateway.Web.Database
{
    public class RiskBatchModel
    {
        public RiskBatchModel()
        {
            Items = new List<RiskBatchGroup>();
        }

        public List<RiskBatchGroup> Items { get; private set; }
    }
}