using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using Absa.Cib.MIT.TaskScheduling.Client.Scheduler;
using Absa.Cib.MIT.TaskScheduling.Models;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Database;
using Gateway.Web.Models.Schedule;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Schedule.Interfaces;
using Gateway.Web.Services.Schedule.Models;
using Gateway.Web.Services.Schedule.Utils;
using Newtonsoft.Json;
using ScheduleGroup = Gateway.Web.Database.ScheduleGroup;

namespace Gateway.Web.Services.Schedule
{
    public abstract class BaseScheduleService<T, TK> : IScheduleService<T>
        where T : BaseScheduleModel
        where TK : BaseScheduleParameters
    {
        protected IRedstoneWebRequestScheduler Scheduler;
        protected ILogger Logger;

        protected BaseScheduleService(IRedstoneWebRequestScheduler scheduler,
            ILoggingService loggingService)
        {
            Scheduler = scheduler;
            Logger = loggingService.GetLogger(this);
        }

        public IList<ModelErrorCollection> ScheduleBatches(T model)
        {
            using (var db = new GatewayEntities())
            {
                var jobKeys = new List<string>();
                var errorCollection = new List<ModelErrorCollection>();
                var parameters = GetParameters(db, model);
                var errors = new ModelErrorCollection();
                try
                {
                    var schedules = Schedule(parameters, db, errorCollection, jobKeys);
                    db.SaveChanges();

                    foreach (var schedule in schedules)
                    {
                        if (!TryScheduleBatch(schedule, parameters, db))
                        {
                            Scheduler.RemoveScheduledWebRequest(schedule.ScheduleKey);
                            if(schedule.RequestConfiguration != null)
                                db.RequestConfigurations.Remove(schedule.RequestConfiguration);
                            db.Schedules.Remove(schedule);
                        }
                    }

                    db.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    Logger.Error(ex, "Unable to save item due to DbEntityValidationException.");
                    foreach (var error in ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors))
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Unable to save item.");
                    errors.Add("Unable to save item: " + ex.Message);
                    HandleException(ex, jobKeys);
                }
                finally
                {
                    errorCollection.Add(errors);
                }

                return errorCollection;
            }
        }

        public void RescheduleBatches(long id, long? groupId = null, string key = null)
        {
            using (var db = new GatewayEntities())
            {
                var schedule = GetSchedule(db, id, key);

                if (schedule.GroupId == null)
                    return;

                if (schedule.Parent != null)
                    return;

                var group = db.ScheduleGroups.Single(x => x.GroupId == (groupId ?? schedule.GroupId.Value));
                var isAsync = true;

                if (schedule.RequestConfiguration?.Arguments != null)
                {
                    var args = JsonConvert.DeserializeObject<IList<Argument>>(schedule.RequestConfiguration.Arguments);
                    var isAsyncArg = args.SingleOrDefault(x => x.Key.ToLower() == "isasync");
                    if (isAsyncArg != null)
                    {
                        isAsync = isAsyncArg.FormatValue.ToLower() == "true";
                    }
                }

                ScheduleBatch(schedule, group, isAsync);
                schedule.GroupId = group.GroupId;
                schedule.IsEnabled = true;

                db.SaveChanges();
            }
        }

        protected abstract TK GetParameters(GatewayEntities db, T model);

        public abstract IList<Database.Schedule> Schedule(TK parameters, GatewayEntities db, IList<ModelErrorCollection> errorCollection,
            IList<string> jobKeys);

        protected virtual Database.Schedule GetSchedule(GatewayEntities db, long id, string key)
        {
            var schedule = db.Schedules
                .Where(x => id == 0 && key != null ? x.ScheduleKey == key : x.ScheduleId == id)
                .Include("RiskBatchConfiguration")
                .Include("RequestConfiguration")
                .Include("ParentSchedule")
                .Include("Children")
                .SingleOrDefault();

            if (schedule != null)
                return schedule;

            schedule = new Database.Schedule { ScheduleKey = key };
            db.Schedules.Add(schedule);

            return schedule;
        }

        protected virtual void AssignSchedule(Database.Schedule entity, TK parameters)
        {
            entity.ScheduleGroup = parameters.Group;
            entity.GroupId = parameters.Group?.GroupId;
        }

        protected virtual void HandleChildSchedules(Database.Schedule entity, TK parameters)
        {
            var existingChildren = entity.Children;
            foreach (var child in existingChildren)
            {
                child.Parent = null;
                child.GroupId = parameters.Group?.GroupId;
            }

            entity.Children.Clear();

            foreach (var child in parameters.Children)
            {
                child.Parent = entity.ScheduleId;
                child.GroupId = null;
            }

            entity.Children = parameters.Children;
        }

        protected virtual void HandleParentSchedule(Database.Schedule entity, TK parameters)
        {
            entity.Parent = parameters.Parent?.ScheduleId;
            entity.ParentSchedule = parameters.Parent;
            entity.ParentSchedule?.Children.Add(entity);
            if (entity.Parent.HasValue)
            {
                entity.GroupId = null;
                entity.ScheduleGroup = null;
            }
        }

        protected virtual bool TryScheduleBatch(Database.Schedule entity, TK parameters, GatewayEntities db)
        {
            try
            {
                ScheduleBatch(entity, entity.ScheduleGroup, parameters.IsAsync);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Unable to save schedule for {entity.TradeSource}");
                return false;
            }
        }

        protected virtual void ScheduleBatch(Database.Schedule entity, ScheduleGroup group, bool isAsync)
        {
            Scheduler.RemoveScheduledWebRequest(entity.ScheduleKey);

            if (group == null)
            {
                Scheduler.RemoveScheduledWebRequest(entity.ParentSchedule.ScheduleKey);
                var parent = GetRequest(entity.ParentSchedule);
                var cron = entity.ParentSchedule.ScheduleGroup.Schedule;
                if (isAsync)
                    Scheduler.ScheduleAsyncWebRequest(parent, entity.ParentSchedule.ScheduleKey, cron);
                else
                    Scheduler.ScheduleWebRequest(parent, entity.ParentSchedule.ScheduleKey, cron);
            }
            else
            {
                var request = GetRequest(entity);
                var cron = group.Schedule;
                if (isAsync)
                    Scheduler.ScheduleAsyncWebRequest(request, entity.ScheduleKey, cron);
                else
                    Scheduler.ScheduleWebRequest(request, entity.ScheduleKey, cron);
            }
        }

        protected virtual void HandleException(Exception ex, IList<string> jobKeys)
        {
            Logger.Error(ex, "Unable to schedule batch.");
            foreach (var jobKey in jobKeys)
            {
                Logger.Error(ex, $"Removing job key, {jobKey}, from schedule.");
                Scheduler.RemoveScheduledWebRequest(jobKey);
            }
        }

        protected abstract RedstoneRequest GetRequest(Database.Schedule schedule, DateTime? businessDate = null);
    }
}