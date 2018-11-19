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
                OutputTag = config.OutputTag,
                OutputType = config.OutputType,
                StateTtlMinutes = config.StateTtlMinutes,
                Type = config.Type,
                //MarketDataMapName = config.MarketDataMapName,
                //TradeSourceType = config.TradeSourceType,
                //ReportingCurrency = config.ReportingCurrency,
                //FundingCurrency = config.FundingCurrency,
            };
        }


        public static RiskBatchConfiguration ToEntity(this BatchConfigModel config)
        {
            return new RiskBatchConfiguration
            {
                ConfigurationId = config.ConfigurationId,
                OutputTag = config.OutputTag,
                OutputType = config.OutputType,
                StateTtlMinutes = config.StateTtlMinutes ?? 0,
                Type = config.Type,
                //MarketDataMapName = config.MarketDataMapName,
                //TradeSourceType = config.TradeSourceType,
                //ReportingCurrency = config.ReportingCurrency,
                //FundingCurrency = config.FundingCurrency,
            };
        }

        public static void UpdateEntity(this RiskBatchConfiguration configuration, BatchConfigModel model)
        {
            configuration.ConfigurationId = model.ConfigurationId;
            configuration.OutputTag = model.OutputTag;
            configuration.OutputType = model.OutputType;
            configuration.StateTtlMinutes = model.StateTtlMinutes ?? 0;
            configuration.Type = model.Type;
            //configuration.MarketDataMapName = model.MarketDataMapName;
            //configuration.TradeSourceType = model.TradeSourceType;
            //configuration.ReportingCurrency = model.ReportingCurrency;
            //configuration.FundingCurrency = model.FundingCurrency;
        }
    }
}