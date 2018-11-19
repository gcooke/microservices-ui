using System;
using System.Collections.Generic;
using System.Linq;
using Absa.Cib.MIT.TaskScheduling.Models;
using Gateway.Web.Database;
using Gateway.Web.Models.Schedule;
using Gateway.Web.Models.Schedule.Input;

namespace Gateway.Web.Services.Schedule.Models
{
    public class BatchScheduleParameters : BaseScheduleParameters
    {
        public IList<RiskBatchConfiguration> Configurations { get; set; }

        public ISet<TradeSourceParameter> TradeSources { get; set; }

        public IDictionary<string,string> Properties { get; set; }

        public override void Populate(GatewayEntities db, BaseScheduleModel model)
        {
            base.Populate(db, model);

            var m = model as ScheduleBatchModel;

            if (m == null)
                throw new Exception($"Model is not of type {typeof(ScheduleBatchModel).Name}");

            Configurations = db.RiskBatchConfigurations
                .Where(x => m.ConfigurationIdList.Contains(x.ConfigurationId.ToString()))
                .ToList();

            TradeSources = GetTradeSources(m.TradeSources.Where(x => !x.IsEmpty()), m.Properties);
            Properties = GetProperties(m.Properties);
            IsAsync = true;
        }

        private ISet<TradeSourceParameter> GetTradeSources(IEnumerable<TradeSourceParameter> tradeSources, IList<Header> properties)
        {
            var tradeSourcesMap = new HashSet<TradeSourceParameter>();
            var fundingCurrency = properties.SingleOrDefault(x => x.Key.ToLower().Replace(" ", "") == "fundingcurrency");
            var reportingCurrency = properties.SingleOrDefault(x => x.Key.ToLower().Replace(" ", "") == "reportingcurrency");

            foreach (var source in tradeSources)
            {
                source.MarketDataMap = source.MarketDataMap ?? "Default";
                source.FundingCurrency = fundingCurrency?.Value ?? "USD";
                source.ReportingCurrency = reportingCurrency?.Value ?? "ZAR";
                tradeSourcesMap.Add(source);
            }

            return tradeSourcesMap;
        }

        private IDictionary<string, string> GetProperties(IEnumerable<Header> properties)
        {
            var propertiesDict = new Dictionary<string,string>();

            foreach (var property in properties)
            {
                propertiesDict.Add(property.Key, property.Value);
            }

            return propertiesDict;
        }
    }

    public class TradeSourceParameter
    {
        public string TradeSourceType { get; }
        public string TradeSource { get; }
        public string Site { get; }
        public string MarketDataMap { get; set; }
        public string FundingCurrency { get; set; }
        public string ReportingCurrency { get; set; }

        public TradeSourceParameter()
        {
        }

        public TradeSourceParameter(string tradeSourceType, string tradeSource, string site)
        {
            TradeSourceType = tradeSourceType;
            TradeSource = tradeSource;
            Site = site;
        }

        public override bool Equals(object obj)
        {
            var item = obj as TradeSourceParameter;

            if (item == null) return false;

            if (item.TradeSourceType == TradeSourceType &&
                item.TradeSource == TradeSource &&
                item.Site == Site)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return $"{TradeSourceType}{TradeSource}{Site}".GetHashCode();
        }

        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(TradeSourceType) ||
                   string.IsNullOrWhiteSpace(TradeSource) ||
                   string.IsNullOrWhiteSpace(Site);
        }
    }
}