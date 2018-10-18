using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Absa.Cib.MIT.TaskScheduling.Client.Scheduler;
using Absa.Cib.MIT.TaskScheduling.Server.Services.Executable;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Database;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Schedule.Models;

namespace Gateway.Web.Services.Schedule
{
    public class ExecutableScheduleService : BaseScheduleService<ScheduleExecutableModel, ExecutableScheduleParameters, ExecutableOptions>
    {
        private readonly IExecutableScheduler _scheduler;

        public ExecutableScheduleService(IExecutableScheduler scheduler, 
            ILoggingService loggingService)
            : base(loggingService)
        {
            _scheduler = scheduler;
        }

        public override IList<Database.Schedule> Schedule(ExecutableScheduleParameters parameters, GatewayEntities db,
            IList<ModelErrorCollection> errorCollection, IList<string> jobKeys)
        {
            var schedules = new List<Database.Schedule>();
            var config = db.ExecutableConfigurations.SingleOrDefault(x => x.ExecutableConfigurationId == parameters.ExecutableConfigurationId);
            if (config == null)
            {
                var isNameUnique = !db.ExecutableConfigurations.Any(x => x.Name == parameters.Name);

                if (!isNameUnique)
                    throw new Exception("Scheduled executable with the same name already exists.");

                config = new ExecutableConfiguration
                {
                    Name = parameters.Name,
                    ExecutablePath = parameters.PathToExe,
                    CommandLineArguments = parameters.Arguments
                };

                db.ExecutableConfigurations.Add(config);
            }
            else
            {
                var isNameUnique = !(parameters.Name != config.Name && db.ExecutableConfigurations.Any(x => x.Name == parameters.Name));

                if (!isNameUnique)
                    throw new Exception("Scheduled executable with the same name already exists.");

                config.Name = parameters.Name;
                config.ExecutablePath = parameters.PathToExe;
                config.CommandLineArguments = parameters.Arguments;
            }

            var key = GenerateKey(config);
            var entity = GetSchedule(db, parameters.ScheduleId, key);
            var errors = parameters.Validate(entity);

            if (errors.Any())
            {
                errorCollection.Add(errors);
                return schedules;
            }

            jobKeys.Add(key);

            AssignSchedule(entity, parameters, config);

            if (parameters.ModifyParent) HandleParentSchedule(entity, parameters);
            if (parameters.ModifyChildren) HandleChildSchedules(entity, parameters);

            schedules.Add(entity);
            return schedules;
        }

        protected override ExecutableScheduleParameters GetParameters(GatewayEntities db, ScheduleExecutableModel model)
        {
            var parameters = new ExecutableScheduleParameters();
            parameters.Populate(db, model);
            return parameters;
        }

        protected override void RemoveSchedule(string key)
        {
            _scheduler.RemoveScheduledExecutable(key);
        }

        protected override ExecutableOptions GetJob(Database.Schedule schedule, DateTime? businessDate = null)
        {
            var options = new ExecutableOptions
            {
                PathToExe = schedule.ExecutableConfiguration?.ExecutablePath,
                ScheduleId = schedule.ScheduleId
            };

            if (schedule.ExecutableConfiguration?.CommandLineArguments != null)
            {
                options.AddArgument(schedule.ExecutableConfiguration?.CommandLineArguments);
            }

            return options;
        }

        protected override void ScheduleTask(ExecutableOptions item, string key, string cron)
        {
            _scheduler.ScheduleExecutable(item, key, cron);
        }

        protected override void ScheduleTaskAsync(ExecutableOptions item, string key, string cron)
        {
            _scheduler.ScheduleExecutable(item, key, cron);
        }

        protected string GenerateKey(ExecutableConfiguration config)
        {
            return $"EXECUTABLE={config.Name}";
        }
        
        protected virtual void AssignSchedule(Database.Schedule entity, ExecutableScheduleParameters parameters, ExecutableConfiguration configuration)
        {
            base.AssignSchedule(entity, parameters);
            entity.ExecutableConfiguration = configuration;
        }
    }
}