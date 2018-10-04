using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Absa.Cib.MIT.TaskScheduling.Client.Scheduler;
using Absa.Cib.MIT.TaskScheduling.Models;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Database;
using Gateway.Web.Enums;
using Gateway.Web.Models.Schedule;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Schedule.Models;
using Gateway.Web.Services.Schedule.Utils;

namespace Gateway.Web.Services.Schedule
{
    public class RedstoneBatchScheduleService : BaseScheduleService<ScheduleBatchModel, BatchScheduleParameters>
    {
        public RedstoneBatchScheduleService(IRedstoneWebRequestScheduler scheduler, 
            ILoggingService loggingService) : 
            base(scheduler, loggingService)
        {
        }

        protected override BatchScheduleParameters GetParameters(GatewayEntities db, ScheduleBatchModel model)
        {
            var parameters = new BatchScheduleParameters();
            parameters.Populate(db, model);
            return parameters;
        }

        public override IList<Database.Schedule> Schedule(BatchScheduleParameters parameters, GatewayEntities db, IList<ModelErrorCollection> errorCollection, IList<string> jobKeys)
        {
            var schedules = new List<Database.Schedule>();
            foreach (var config in parameters.Configurations)
            {
                foreach (var tradeSource in parameters.TradeSources)
                {
                    var key = GenerateKey(config, tradeSource);
                    var entity = GetSchedule(db, parameters.ScheduleId, key);
                    var errors = parameters.Validate(entity);

                    if (errors.Any())
                    {
                        errorCollection.Add(errors);
                        continue;
                    }

                    jobKeys.Add(key);

                    AssignSchedule(entity, parameters, config, tradeSource);

                    if (parameters.ModifyParent) HandleParentSchedule(entity, parameters);
                    if (parameters.ModifyChildren) HandleChildSchedules(entity, parameters);

                    schedules.Add(entity);
                }
            }

            return schedules;
        }

        protected override RedstoneRequest GetRequest(Database.Schedule schedule, DateTime? businessDate = null)
        {
            return schedule.ToRequest(businessDate);
        }

        protected string GenerateKey(RiskBatchConfiguration configuration, string tradeSource)
        {
            return $"BATCH={configuration.ConfigurationId}-TRADESOURCE={tradeSource.ToUpper().Trim()}";
        }

        protected virtual void AssignSchedule(Database.Schedule entity, BatchScheduleParameters parameters, RiskBatchConfiguration configuration, string tradeSource)
        {
            base.AssignSchedule(entity, parameters);
            entity.RiskBatchConfigurationId = configuration.ConfigurationId;
            entity.TradeSource = tradeSource.Trim();

            TradeSourceType tradeSourceType;
            if (Enum.TryParse(configuration.TradeSourceType, out tradeSourceType))
                return;

            if (tradeSourceType == TradeSourceType.Site)
            {
                entity.Site = tradeSource;
            }
        }
    }
}