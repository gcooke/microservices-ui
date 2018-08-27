using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Absa.Cib.MIT.TaskScheduling.Client.Scheduler;
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
        private readonly IRedstoneWebRequestScheduler _scheduler;

        public ScheduleDataService(
            IRedstoneWebRequestScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public IList<ScheduleTask> GetScheduleTasks(IEnumerable<long> scheduleIdList)
        {
            using (var db = new GatewayEntities())
            {
                return db.Schedules
                    .Where(x => scheduleIdList.Contains(x.ScheduleId))
                    .Include("RiskBatchConfiguration")
                    .Include("RequestConfiguration")
                    .Include("ParentSchedule")
                    .Include("Children")
                    .ToList()
                    .Select(x => x.ToModel())
                    .ToList();
            }
        }

        public ScheduleTask GetScheduleTask(long id)
        {
            using (var db = new GatewayEntities())
            {
                var entity = db.Schedules
                    .Include("RiskBatchConfiguration")
                    .Include("RequestConfiguration")
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
            using (var db = new GatewayEntities())
            {
                var entity = db.Schedules
                    .Include("RiskBatchConfiguration")
                    .Include("RequestConfiguration")
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
            using (var db = new GatewayEntities())
            {
                return db.Schedules
                    .Where(x => scheduleIdList.Contains(x.ScheduleId))
                    .Include("RiskBatchConfiguration")
                    .Include("RequestConfiguration")
                    .Include("ParentSchedule")
                    .Include("Children")
                    .ToList();
            }
        }


        public IDictionary<string, string> GetDailyStatuses(DateTime now)
        {
            using (var db = new GatewayEntities())
            {
                var data = new Dictionary<string, string>();
                var date = new DateTime(now.Year, now.Month, now.Day).Date;
                while (now - date <= TimeSpan.FromDays(30))
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
            db = db ?? new GatewayEntities();
            var entity = db.Schedules
                .Include("RiskBatchConfiguration")
                .Include("RequestConfiguration")
                .Include("ParentSchedule")
                .Include("Children")
                .SingleOrDefault(x => x.ScheduleId == id);

            if (entity == null)
            {
                throw new Exception($"Unable to find schedule with ID {id}");
            }

            foreach (var child in entity.Children)
            {
                child.Parent = entity.Parent;
                child.GroupId = entity.GroupId;
                ScheduleTask(child, entity.ScheduleGroup);
            }

            _scheduler.RemoveScheduledWebRequest(entity.ScheduleKey);

            db.ScheduledJobs.RemoveRange(entity.ScheduledJobs);
            if(entity.RequestConfigurationId != null)
                db.RequestConfigurations.Remove(entity.RequestConfiguration);

            db.Schedules.Remove(entity);

            if (!saveChanges)
                return;

            db.SaveChanges();
            db.Dispose();
        }

        public void DeleteForConfiguration(long id, GatewayEntities db = null, bool saveChanges = true)
        {
            db = db ?? new GatewayEntities();
            var entities = db.Schedules
                .Where(x => x.RiskBatchConfigurationId == id)
                .Include("RiskBatchConfiguration")
                .Include("RequestConfiguration")
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

        public void RerunTask(long id, DateTime businessDate)
        {
            using (var db = new GatewayEntities())
            {
                var batch = db.Schedules.SingleOrDefault(x => x.ScheduleId == id);

                if (batch == null)
                    return;

                _scheduler.EnqueueAsyncWebRequest(batch.ToRequest(businessDate));
            }
        }

        public void RerunTaskGroup(long id, DateTime businessDate)
        {
            using (var db = new GatewayEntities())
            {
                var batches = db.Schedules
                    .Where(x => x.GroupId == id)
                    .Include("RiskBatchConfiguration")
                    .Include("RequestConfiguration")
                    .Include("ParentSchedule")
                    .Include("Children")
                    .ToList();

                if (!batches.Any())
                    return;

                foreach (var schedule in batches)
                {
                    _scheduler.EnqueueAsyncWebRequest(schedule.ToRequest(businessDate));
                }
            }
        }

        private void ScheduleTask(Database.Schedule entity, Database.ScheduleGroup group)
        {
            _scheduler.RemoveScheduledWebRequest(entity.ScheduleKey);

            if (group == null)
            {
                _scheduler.RemoveScheduledWebRequest(entity.ParentSchedule.ScheduleKey);
                var parent = entity.ParentSchedule.ToRequest();
                var cron = entity.ParentSchedule.ScheduleGroup.Schedule;
                _scheduler.ScheduleAsyncWebRequest(parent, entity.ParentSchedule.ScheduleKey, cron);
            }
            else
            {
                var request = entity.ToRequest();
                var cron = group.Schedule;
                _scheduler.ScheduleAsyncWebRequest(request, entity.ScheduleKey, cron);
            }
        }
    }
}