using Gateway.Web.Database;
using Gateway.Web.Models.Batches;

namespace Gateway.Web.Services.Batches.Utils
{
    public static class BatchEx
    {
        public static BatchConfigModel ToModel(this RiskBatchConfiguration config)
        {
            return new BatchConfigModel
            {
                ConfigurationId = config.ConfigurationId,
                MarketDataMapName = config.MarketDataMapName,
                OutputTag = config.OutputTag,
                OutputType = config.OutputType,
                StateTtlMinutes = config.StateTtlMinutes,
                TradeSourceType = config.TradeSourceType,
                Type = config.Type,
                ScheduleCount = config.Schedules.Count
            };
        }


        public static RiskBatchConfiguration ToEntity(this BatchConfigModel config)
        {
            return new RiskBatchConfiguration
            {
                ConfigurationId = config.ConfigurationId,
                MarketDataMapName = config.MarketDataMapName,
                OutputTag = config.OutputTag,
                OutputType = config.OutputType,
                StateTtlMinutes = config.StateTtlMinutes ?? 0,
                TradeSourceType = config.TradeSourceType,
                Type = config.Type
            };
        }

        public static void UpdateEntity(this RiskBatchConfiguration configuration, BatchConfigModel model)
        {
            configuration.ConfigurationId = model.ConfigurationId;
            configuration.MarketDataMapName = model.MarketDataMapName;
            configuration.OutputTag = model.OutputTag;
            configuration.OutputType = model.OutputType;
            configuration.StateTtlMinutes = model.StateTtlMinutes ?? 0;
            configuration.TradeSourceType = model.TradeSourceType;
            configuration.Type = model.Type;
        }
    }
}