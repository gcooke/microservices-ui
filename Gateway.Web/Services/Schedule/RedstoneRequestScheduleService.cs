using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Absa.Cib.MIT.TaskScheduling.Client.Scheduler;
using Absa.Cib.MIT.TaskScheduling.Models;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Database;
using Gateway.Web.Models.Schedule;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Schedule.Models;
using Gateway.Web.Services.Schedule.Utils;
using Newtonsoft.Json;

namespace Gateway.Web.Services.Schedule
{
    public class RedstoneRequestScheduleService : BaseScheduleService<ScheduleWebRequestModel, RedstoneRequestParameters>
    {
        public RedstoneRequestScheduleService(IRedstoneWebRequestScheduler scheduler, 
            ILoggingService loggingService) : 
            base(scheduler, loggingService)
        {
        }

        protected override RedstoneRequestParameters GetParameters(GatewayEntities db, ScheduleWebRequestModel model)
        {
            var parameters = new RedstoneRequestParameters();
            parameters.Populate(db, model);
            return parameters;
        }

        public override void Schedule(RedstoneRequestParameters parameters, GatewayEntities db, IList<ModelErrorCollection> errorCollection, IList<string> jobKeys)
        {
            var config = db.RequestConfigurations.SingleOrDefault(x => x.RequestConfigurationId == parameters.RequestConfigurationId);
            if (config == null)
            {
                var isNameUnique = !db.RequestConfigurations.Any(x => x.Name == parameters.Name);

                if (!isNameUnique)
                    throw new Exception("Scheduled web request with the same name already exists.");

                config = new RequestConfiguration
                {
                    Name = parameters.Name,
                    Arguments = JsonConvert.SerializeObject(parameters.Arguments),
                    Headers = JsonConvert.SerializeObject(parameters.Headers),
                    Payload = parameters.Payload,
                    Url = parameters.Url,
                    Verb = parameters.Verb
                };

                db.RequestConfigurations.Add(config);
            }
            else
            {
                var isNameUnique = !(parameters.Name != config.Name && db.RequestConfigurations.Any(x => x.Name == parameters.Name));

                if (!isNameUnique)
                    throw new Exception("Scheduled web request with the same name already exists.");

                config.Name = parameters.Name;
                config.Arguments = JsonConvert.SerializeObject(parameters.Arguments);
                config.Headers = JsonConvert.SerializeObject(parameters.Headers);
                config.Payload = parameters.Payload;
                config.Url = parameters.Url;
                config.Verb = parameters.Verb;
            }

            var key = GenerateKey(config);
            var entity = GetSchedule(db, key);
            var errors = parameters.Validate(entity);

            if (errors.Any())
            {
                errorCollection.Add(errors);
                return;
            }

            jobKeys.Add(key);

            AssignSchedule(entity, parameters, config);

            if (parameters.ModifyParent) HandleParentSchedule(entity, parameters);
            if (parameters.ModifyChildren) HandleChildSchedules(entity, parameters);

            if (!TrySaveSchedule(entity, parameters, db))
            {
                Scheduler.RemoveScheduledWebRequest(key);
                db.RequestConfigurations.Remove(config);
            }
        }

        protected string GenerateKey(RequestConfiguration configuration)
        {
            return $"REQUEST={configuration.Name}";
        }

        protected override RedstoneRequest GetRequest(Database.Schedule schedule, DateTime? businessDate = null)
        {
            return schedule.ToRequest(businessDate);
        }

        protected virtual void AssignSchedule(Database.Schedule entity, RedstoneRequestParameters parameters, RequestConfiguration configuration)
        {
            base.AssignSchedule(entity, parameters);
            entity.RequestConfiguration = configuration;
        }

    }
}