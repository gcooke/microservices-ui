using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Database;
using Gateway.Web.Models.Schedule;
using Gateway.Web.Models.Schedule.Input;

namespace Gateway.Web.Services.Schedule.Models
{
    public class BatchScheduleParameters : BaseScheduleParameters
    {
        public IList<RiskBatchConfiguration> Configurations { get; set; }

        public IDictionary<string,string> TradeSources { get; set; }

        public override void Populate(GatewayEntities db, BaseScheduleModel model)
        {
            base.Populate(db, model);

            var m = model as ScheduleBatchModel;

            if (m == null)
                throw new Exception($"Model is not of type {typeof(ScheduleBatchModel).Name}");

            Configurations = db.RiskBatchConfigurations
                .Where(x => m.ConfigurationIdList.Contains(x.ConfigurationId.ToString()))
                .ToList();

            TradeSources = GetTradeSources(m.TradeSources);
            IsAsync = true;
        }

        private IDictionary<string, string> GetTradeSources(string tradeSources)
        {
            var sources = tradeSources.Split(',');
            var tradeSourcesMap = new Dictionary<string, string>();

            foreach (var source in sources)
            {
                var parts = source.Split(';');

                if (tradeSourcesMap.ContainsKey(parts[0]))
                    continue;
                
                tradeSourcesMap.Add(parts[0], parts.Length > 1 ? parts[1] : null);
            }

            return tradeSourcesMap;
        }
    }
}