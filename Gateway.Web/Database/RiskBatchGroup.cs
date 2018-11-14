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

        public string Name { get; set; }

        public int TotalRuns => Items.Count;

        [JsonIgnore]
        public int CompleteRuns
        {
            get { return Items.Count(x => x.State == StateItemState.Okay); }
        }

        public List<RiskBatchResult> Items { get; set; }

        [JsonIgnore]
        public List<RiskBatchResult> IncompleteItems
        {
            get { return Items.Where(x => x.State != StateItemState.Okay).ToList(); }
        }
    }
}