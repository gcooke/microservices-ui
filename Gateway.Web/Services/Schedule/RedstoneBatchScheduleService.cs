using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Absa.Cib.MIT.TaskScheduling.Client.Scheduler;
using Absa.Cib.MIT.TaskScheduling.Models;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Database;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Schedule.Models;
using Gateway.Web.Services.Schedule.Utils;
using Newtonsoft.Json;

namespace Gateway.Web.Services.Schedule
{
    public class RedstoneBatchScheduleService : BaseScheduleService<ScheduleBatchModel, BatchScheduleParameters, RedstoneRequest>
    {
        private readonly IRedstoneWebRequestScheduler _scheduler;

        public RedstoneBatchScheduleService(IRedstoneWebRequestScheduler scheduler, 
            ILoggingService loggingService,
            ISystemInformation systemInformation) : 
            base(loggingService, systemInformation)
        {
            _scheduler = scheduler;
        }

        protected override BatchScheduleParameters GetParameters(GatewayEntities db, ScheduleBatchModel model)
        {
            var parameters = new BatchScheduleParameters();
            parameters.Populate(db, model);
            return parameters;
        }

        protected override void RemoveSchedule(string key)
        {
            _scheduler.RemoveScheduledWebRequest(key);
        }

        public override IList<Database.Schedule> Schedule(BatchScheduleParameters parameters, GatewayEntities db, IList<ModelErrorCollection> errorCollection, IList<string> jobKeys)
        {
            var schedules = new List<Database.Schedule>();
            foreach (var config in parameters.Configurations)
            {
                foreach (var tradeSourceParameter in parameters.TradeSources)
                {
                    var serializedProperties = JsonConvert.SerializeObject(parameters.Properties);
                    var key = GenerateKey(config, tradeSourceParameter.TradeSource, tradeSourceParameter.IsLive, serializedProperties);
                    var entity = GetSchedule(db, parameters.ScheduleId, key);
                    var errors = parameters.Validate(entity);

                    if (errors.Any())
                    {
                        errorCollection.Add(errors);
                        continue;
                    }

                    jobKeys.Add(key);

                    var riskBatchSchedule = GetRiskBatchSchedule(db, config.ConfigurationId, tradeSourceParameter, serializedProperties) ?? new RiskBatchSchedule();
                    if (riskBatchSchedule.RiskBatchScheduleId == 0 && entity.ScheduleId != 0)
                        entity = new Database.Schedule() { ScheduleKey = key };

                    AssignSchedule(entity, riskBatchSchedule, parameters, tradeSourceParameter, config.ConfigurationId);

                    if (parameters.ModifyParent) HandleParentSchedule(entity, parameters);
                    if (parameters.ModifyChildren) HandleChildSchedules(entity, parameters);

                    if (riskBatchSchedule.RiskBatchScheduleId == 0)
                    {
                        riskBatchSchedule.Schedules.Add(entity);
                        db.RiskBatchSchedules.Add(riskBatchSchedule);
                    }

                    schedules.Add(entity);
                }
            }

            return schedules;
        }

        private RiskBatchSchedule GetRiskBatchSchedule(GatewayEntities db, long configurationId, TradeSourceParameter tradeSource, string additionalProperties)
        {
            return db.RiskBatchSchedules
                .Where(x => x.RiskBatchConfigurationId == configurationId)
                .Where(x => x.TradeSourceType == tradeSource.TradeSourceType)
                .Where(x => x.TradeSource == tradeSource.TradeSource)
                .Where(x => x.Site == tradeSource.Site)
                .Where(x => x.IsLive == tradeSource.IsLive)
                .SingleOrDefault(x => x.AdditionalProperties.ToUpper() == additionalProperties.ToUpper());
        }

        protected override RedstoneRequest GetJob(Database.Schedule schedule, DateTime? businessDate = null)
        {
            return schedule.ToRequest(schedule.RiskBatchSchedule?.IsLive ?? false, businessDate);
        }

        protected override void ScheduleTask(RedstoneRequest item, string key, string cron)
        {
            _scheduler.ScheduleWebRequest(item, key, cron);
        }

        protected override void ScheduleTaskAsync(RedstoneRequest item, string key, string cron)
        {
            _scheduler.ScheduleAsyncWebRequest(item, key, cron);
        }

        protected string GenerateKey(RiskBatchConfiguration configuration, string tradeSource, bool isLive, string serializedProperties)
        {
            var key = $"BATCH={configuration.ConfigurationId}-TRADESOURCE={tradeSource.ToUpper().Trim()}.PROPS={serializedProperties.Trim()}";
            if (isLive)
                key = $"{key}-LIVE";

            if (key.Length >= 50)
            {
                var shortenedTradeSources = tradeSource.ToUpper().Trim().GetHashCode();
                var shortenedProps = serializedProperties.ToUpper().Trim().GetHashCode();
                key = $"BATCH={configuration.ConfigurationId}-TRADESOURCE={shortenedTradeSources}.PROPS={shortenedProps}";
                if (isLive)
                    key = $"{key}-LIVE";
            }

            if (key.Length >= 50)
            {
                key = $"BATCH={key.GetHashCode()}";
            }

            return key;
        }

        protected virtual void AssignSchedule(Database.Schedule entity, RiskBatchSchedule riskBatchSchedule, BatchScheduleParameters parameters, TradeSourceParameter tradeSourceParameter, long configurationId)
        {
            base.AssignSchedule(entity, parameters);
            riskBatchSchedule.RiskBatchConfigurationId = configurationId;
            riskBatchSchedule.TradeSourceType = tradeSourceParameter.TradeSourceType.Trim();
            riskBatchSchedule.TradeSource = tradeSourceParameter.TradeSource.Trim();
            riskBatchSchedule.Site = tradeSourceParameter.Site?.Trim() ?? tradeSourceParameter.TradeSource.Trim();
            riskBatchSchedule.MarketDataMap = tradeSourceParameter.MarketDataMap?.Trim();
            riskBatchSchedule.FundingCurrency = tradeSourceParameter.FundingCurrency?.Trim();
            riskBatchSchedule.ReportingCurrency = tradeSourceParameter.ReportingCurrency?.Trim();
            riskBatchSchedule.AdditionalProperties = JsonConvert.SerializeObject(parameters.Properties);
            riskBatchSchedule.IsLive = tradeSourceParameter.IsLive;
        }
    }
}