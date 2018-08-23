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
        private readonly IScheduleGroupService _scheduleGroupService;
        private readonly IRedstoneWebRequestScheduler _scheduler;

        public ScheduleDataService(IScheduleGroupService scheduleGroupService,
            IRedstoneWebRequestScheduler scheduler)
        {
            _scheduleGroupService = scheduleGroupService;
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

        public IList<ScheduleGroup> GetScheduleGroups(DateTime date, string searchTerm = null)
        {
            using (var db = new GatewayEntities())
            {
                var startDate = date.Date;
                var endDate = startDate.AddHours(24).AddMinutes(-1);
                var groups = _scheduleGroupService.GetGroups(searchTerm);
                var runGroups = new List<ScheduleGroup>();

                string status = null;
                if (!string.IsNullOrWhiteSpace(searchTerm) && searchTerm.Contains("status:"))
                {
                    var statusIndex = searchTerm.IndexOf("status:", StringComparison.Ordinal);
                    if (statusIndex > 0)
                    {
                        searchTerm = searchTerm.Substring(0, statusIndex - 1).Trim();
                    }
                    status = searchTerm.Substring(statusIndex + "status:".Length);
                }

                foreach (var group in groups)
                {
                    var crontabSchedule = CrontabSchedule.Parse(group.Schedule);
                    var occurrences = crontabSchedule
                        .GetNextOccurrences(startDate, endDate)
                        .Select(x => x.ToLocalTime())
                        .ToList();

                    if(!occurrences.Any())
                        continue;

                    group.FriendScheduledTime = occurrences.First().ToString("hh:mm tt");

                    foreach (var batch in group.Tasks)
                    {
                        var jobList = db.ScheduledJobs
                            .Where(x => x.ScheduleId == batch.ScheduleId)
                            .Where(x => x.BusinessDate >= startDate && x.BusinessDate <= endDate)
                            .Where(x => status == null || x.Status.ToLower() == status.ToLower())
                            .ToList();

                        var lastJob = jobList.LastOrDefault();

                        if (lastJob == null)
                        {
                            batch.Status = "Not Started";
                            continue;
                        }

                        batch.Status = lastJob.Status;
                        batch.Retries = jobList.Count - 1;
                        batch.RequestId = lastJob.RequestId;
                        batch.StartedAt = lastJob.StartedAt;
                        batch.FinishedAt = lastJob.FinishedAt;
                    }

                    runGroups.Add(group);
                }

                return runGroups;
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
            db.RequestConfigurations = null;

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