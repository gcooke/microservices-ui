using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Models.Home;
using Newtonsoft.Json;

namespace Gateway.Web.Database
{
    public class RiskBatchGroup
    {
        public RiskBatchGroup(string name)
        {
            Name = name;
            Items = new List<RiskBatchResult>();
        }

        public string Name { get; private set; }
        public List<RiskBatchResult> Items { get; private set; }

        public long CompleteRuns
        {
            get
            {
                return Items.Count(i => i.State == nameof(StateItemState.Okay));
            }
        }

        public long TotalRuns
        {
            get { return Items.Count(i => !string.IsNullOrEmpty(i.Name)); }
        }
    }
}