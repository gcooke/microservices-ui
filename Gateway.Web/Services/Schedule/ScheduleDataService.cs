using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using Absa.Cib.MIT.TaskScheduling.Client.Scheduler;
using Bagl.Cib.MIT.IoC;
using Gateway.Web.Database;
using Gateway.Web.Models.Schedule.Output;
using Gateway.Web.Services.Schedule.Interfaces;
using Gateway.Web.Services.Schedule.Utils;
using NCrontab;
using ScheduleGroup = Gateway.Web.Models.Schedule.Output.ScheduleGroup;

namespace Gateway.Web.Services.Schedule
{
    public class ScheduleDataService : IScheduleDataService
    {
        private readonly IExecutableScheduler _executableScheduler;
        private readonly IRedstoneWebRequestScheduler _scheduler;
        private readonly string ConnectionString;

        public ScheduleDataService(IExecutableScheduler executableScheduler, 
            IRedstoneWebRequestScheduler scheduler,
            ISystemInformation systemInformation)
        {
            _executableScheduler = executableScheduler;
            _scheduler = scheduler;
            ConnectionString = systemInformation.GetConnectionString("GatewayDatabase", "Database.PnRFO_Gateway");
        }

        public IList<ScheduleTask> GetScheduleTasks(IEnumerable<long> scheduleIdList)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                return db.Schedules
                    .Where(x => scheduleIdList.Contains(x.ScheduleId))
                    .Include("RiskBatchSchedule")
                    .Include("RiskBatchSchedule.RiskBatchConfiguration")
                    .Include("RequestConfiguration")
                    .Include("ExecutableConfiguration")
                    .Include("ParentSchedule")
                    .Include("Children")
                    .ToList()
                    .Select(x => x.ToModel())
                    .ToList();
            }
        }

        public ScheduleTask GetScheduleTask(long id)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var entity = db.Schedules
                    .Include("RiskBatchSchedule")
                    .Include("RiskBatchSchedule.RiskBatchConfiguration")
                    .Include("RequestConfiguration")
                    .Include("ExecutableConfiguration")
                    .Include("ParentSchedule")
                    .Include("Children")
                    .SingleOrDefault(x => x.ScheduleId == id);

                if (entity == null)
                    throw new Exception($"Unable to get batch schedule with ID {id}.");

                return entity.ToModel();
            }
        }

        public Database.Schedule GetSchedule(long id)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var entity = db.Schedules
                    .Include("RiskBatchSchedule")
                    .Include("RiskBatchSchedule.RiskBatchConfiguration")
                    .Include("RequestConfiguration")
                    .Include("ExecutableConfiguration")
                    .Include("ParentSchedule")
                    .Include("Children")
                    .SingleOrDefault(x => x.ScheduleId == id);

                if (entity == null)
                    throw new Exception($"Unable to get batch schedule with ID {id}.");

                return entity;
            }
        }

        public IList<Database.Schedule> GetSchedules(IEnumerable<long> scheduleIdList)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                return db.Schedules
                    .Where(x => scheduleIdList.Contains(x.ScheduleId))
                    .Include("RiskBatchSchedule")
                    .Include("RiskBatchSchedule.RiskBatchConfiguration")
                    .Include("RequestConfiguration")
                    .Include("ExecutableConfiguration")
                    .Include("ParentSchedule")
                    .Include("Children")
                    .ToList();
            }
        }

        public IDictionary<string, string> GetDailyStatuses(DateTime now)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var data = new Dictionary<string, string>();
                var date = new DateTime(now.Year, now.Month, now.Day).Date;
                while (now - date <= TimeSpan.FromDays(12))
                {
                    var businessDate = date;
                    var jobs = db.ScheduledJobs.Where(x => x.BusinessDate == businessDate)
                        .GroupBy(x => x.ScheduleId)
                        .ToDictionary(x => x.Key, x => x.ToList());

                    var key = date.ToString("yyyy-MM-dd");
                    data.Add(key, "");
                    date = date.AddDays(-1);

                    foreach (var job in jobs)
                    {
                        var allJobs = job.Value.OrderByDescending(x => x.StartedAt);

                        if (!allJobs.Any())
                            continue;

                        var lastJob = allJobs.First();

                        if (lastJob.Status == "Failed")
                        {
                            data[key] = "Failed";
                        }
                        else if (lastJob.Status == "Succeeded" && allJobs.Count() > 1)
                        {
                            data[key] = "Warning";
                        }
                        else if (lastJob.Status == "Succeeded" && allJobs.Count() == 1)
                        {
                            data[key] = "Succeeded";
                        }
                        else
                        {
                            data[key] = "";
                        }

                        if (data[key] == "Failed" || data[key] == "Warning")
                        {
                            break;
                        }
                    }
                }
                return data;
            }
        }

        public void DeleteSchedule(long id, GatewayEntities db = null, bool saveChanges = true)
        {
            db = db ?? new GatewayEntities(ConnectionString);
            var entity = db.Schedules
                .Where(x => x.ScheduleId == id)
                .Include("RiskBatchSchedule")
                .Include("RiskBatchSchedule.RiskBatchConfiguration")
                .Include("RequestConfiguration")
                .Include("ExecutableConfiguration")
                .Include("ParentSchedule")
                .Include("Children")
                .SingleOrDefault();

            if (entity == null)
            {
                throw new Exception($"Unable to find schedule with ID {id}");
            }

            var requestConfigurationId = entity.RequestConfigurationId;
            var requestConfiguration = entity.RequestConfiguration;

            foreach (var child in entity.Children)
            {
                child.Parent = entity.Parent;
                child.GroupId = entity.GroupId;
                ScheduleTask(child, entity.ScheduleGroup);
            }

            _scheduler.RemoveScheduledWebRequest(entity.ScheduleKey);

            var scheduledJobs = entity.ScheduledJobs.ToList();
            foreach (var scheduledJob in scheduledJobs)
            {
                db.ScheduledJobs.Remove(scheduledJob);
            }

            if (entity.RiskBatchScheduleId != null)
                db.RiskBatchSchedules.Remove(entity.RiskBatchSchedule);

            db.Schedules.Remove(entity);

            if (requestConfigurationId != null)
                db.RequestConfigurations.Remove(requestConfiguration);

            if (!saveChanges)
                return;

            db.SaveChanges();
            db.Dispose();
        }

        public void DeleteForConfiguration(long id, GatewayEntities db = null, bool saveChanges = true)
        {
            db = db ?? new GatewayEntities(ConnectionString);
            var entities = db.Schedules
                .Where(x => x.RiskBatchSchedule != null && x.RiskBatchSchedule.RiskBatchConfigurationId == id)
                .Include("RiskBatchSchedule")
                .Include("RiskBatchSchedule.RiskBatchConfiguration")
                .Include("RequestConfiguration")
                .Include("ExecutableConfiguration")
                .Include("ParentSchedule")
                .Include("Children");

            foreach (var schedule in entities)
            {
                DeleteSchedule(schedule.ScheduleId, db, false);
            }

            if (!saveChanges)
                return;

            db.SaveChanges();
            db.Dispose();
        }

        public void RerunTask(long id, DateTime businessDate, bool includeChildren = true)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var batch = db.Schedules.SingleOrDefault(x => x.ScheduleId == id);

                if (batch == null)
                    return;

                if (batch.ExecutableConfiguration != null)
                {
                    _executableScheduler.EnqueueExecutable(batch.ToExecutableOptions());
                    return;
                }

                _scheduler.EnqueueAsyncWebRequest(batch.ToRequest(batch.RiskBatchSchedule?.IsLive ?? false, businessDate, includeChildren));
            }
        }

        public void RerunTaskGroup(long id, DateTime businessDate, string searchTerm)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var batches = db.Schedules
                    .Include("RiskBatchSchedule")
                    .Include("RiskBatchSchedule.RiskBatchConfiguration")
                    .Include("RequestConfiguration")
                    .Include("ExecutableConfiguration")
                    .Include("ParentSchedule")
                    .Include("Children")
                    .Where(GetSearchCriteria(searchTerm))
                    .Where(x => x.GroupId == id)
                    .ToList();

                if (!batches.Any())
                    return;

                foreach (var schedule in batches)
                {
                    _scheduler.EnqueueAsyncWebRequest(schedule.ToRequest(schedule.RiskBatchSchedule?.IsLive ?? false, businessDate));
                }
            }
        }

        public void DisableSchedule(long id)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var schedule = db.Schedules
                    .Where(x => x.ScheduleId == id)
                    .Include("RiskBatchSchedule")
                    .Include("RiskBatchSchedule.RiskBatchConfiguration")
                    .Include("RequestConfiguration")
                    .Include("ExecutableConfiguration")
                    .Include("ParentSchedule")
                    .Include("Children")
                    .SingleOrDefault();

                if (schedule == null)
                    return;

                _scheduler.RemoveScheduledWebRequest(schedule.ScheduleKey);

                schedule.IsEnabled = false;
                db.SaveChanges();
            }
        }

        private void ScheduleTask(Database.Schedule entity, Database.ScheduleGroup group)
        {
            _scheduler.RemoveScheduledWebRequest(entity.ScheduleKey);

            if (group == null)
            {
                _scheduler.RemoveScheduledWebRequest(entity.ParentSchedule.ScheduleKey);
                var parent = entity.ParentSchedule.ToRequest(entity.RiskBatchSchedule?.IsLive ?? false);
                var cron = entity.ParentSchedule.ScheduleGroup.Schedule;
                _scheduler.ScheduleAsyncWebRequest(parent, entity.ParentSchedule.ScheduleKey, cron);
            }
            else
            {
                var request = entity.ToRequest(entity.RiskBatchSchedule?.IsLive ?? false);
                var cron = group.Schedule;
                _scheduler.ScheduleAsyncWebRequest(request, entity.ScheduleKey, cron);
            }
        }

        private Func<Database.Schedule, bool> GetSearchCriteria(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return x => true;

            var terms = s.ToLower().Split(' ');
            return x => terms.Any(y => (x.RiskBatchSchedule != null && x.RiskBatchSchedule.TradeSource != null && x.RiskBatchSchedule.TradeSource.ToLower().Contains(y)) ||
                                       (x.RiskBatchSchedule != null && x.RiskBatchSchedule.RiskBatchConfiguration.Type.ToLower().Contains(y)) ||
                                       (x.RequestConfiguration != null && x.RequestConfiguration.Name.ToLower().Contains(y)));
        }
    }
}