using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Gateway.Web.Database;
using Gateway.Web.Models.Schedule;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Schedule.Interfaces;
using Gateway.Web.Services.Schedule.Utils;
using Gateway.Web.Utils;
using NCrontab;
using ScheduleGroup = Gateway.Web.Models.Schedule.Output.ScheduleGroup;

namespace Gateway.Web.Services.Schedule
{
    public class ScheduleGroupService : IScheduleGroupService
    {
        private readonly IScheduleDataService _scheduleDataService;

        public ScheduleGroupService(IScheduleDataService scheduleDataService)
        {
            _scheduleDataService = scheduleDataService;
        }

        public IList<ScheduleGroup> GetScheduleGroups(DateTime date, string searchTerm = null)
        {
            using (var db = new GatewayEntities())
            {
                var startDate = date.Date;
                var endDate = startDate.AddHours(24).AddMinutes(-1);
                var groups = GetGroups(searchTerm);
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

                    if (!occurrences.Any())
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


        public IList<ScheduleGroup> GetGroups(string searchTerm = null)
        {
            using (var db = new GatewayEntities())
            {
                var groups = db.ScheduleGroups
                    .Include("Schedules")
                    .ToList();

                var groupModels = groups
                    .Select(x => x.ToModel())
                    .ToList();

                foreach (var group in groups)
                {
                    var groupModel = groupModels.Single(x => x.GroupId == group.GroupId);
                    var rootChildren = group.Schedules
                        .Where(GetSearchCriteria(searchTerm))
                        .Where(x => !x.Parent.HasValue);

                    foreach (var schedule in rootChildren)
                    {
                        groupModel.Tasks.Add(schedule.ToModel());

                        if (schedule.Children.Any())
                        {
                            AddChildren(schedule, groupModel, searchTerm);
                        }
                    }
                }

                return groupModels;
            }
        }

        public IList<ScheduleGroup> GetGroups(IList<long> idList)
        {
            using (var db = new GatewayEntities())
            {
                var groups = db.ScheduleGroups
                    .Where(x => idList.Contains(x.GroupId))
                    .Include("Schedules")
                    .ToList();

                var groupModels = groups
                    .Select(x => x.ToModel())
                    .ToList();

                foreach (var group in groups)
                {
                    var groupModel = groupModels.Single(x => x.GroupId == group.GroupId);
                    var rootChildren = group.Schedules
                        .Where(x => !x.Parent.HasValue);

                    foreach (var schedule in rootChildren)
                    {
                        groupModel.Tasks.Add(schedule.ToModel());

                        if (schedule.Children.Any())
                        {
                            AddChildren(schedule, groupModel, null);
                        }
                    }
                }

                return groupModels;
            }
        }

        public ScheduleGroup GetGroup(long id)
        {
            using (var db = new GatewayEntities())
            {
                var group = db.ScheduleGroups
                    .SingleOrDefault(x => x.GroupId == id);

                if (group == null)
                    throw new Exception($"No group found with ID {id}");

                var groupModel = group.ToModel();
                var rootChildren = group.Schedules
                    .Where(x => !x.Parent.HasValue);

                foreach (var schedule in rootChildren)
                {
                    groupModel.Tasks.Add(schedule.ToModel());

                    if (schedule.Children.Any())
                    {
                        AddChildren(schedule, groupModel, null);
                    }
                }

                return groupModel;
            }
        }


        private void AddChildren(Database.Schedule parent, ScheduleGroup groupQuick, string searchTerm)
        {
            var children = parent.Children
                .Where(GetSearchCriteria(searchTerm));

            foreach (var schedule in children)
            {
                groupQuick.Tasks.Add(schedule.ToModel());

                if (schedule.Children.Any())
                {
                    AddChildren(schedule, groupQuick, searchTerm);
                }
            }
        }

        public long CreateOrUpdate(string cron, string name = null)
        {
            using (var db = new GatewayEntities())
            {
                var entity = db.ScheduleGroups.SingleOrDefault(x => x.Schedule == cron);

                if(name == null)
                    name = CronExpressionDescriptor.ExpressionDescriptor.GetDescription(cron);

                cron = CronTabExpression.Parse(cron).ToUtcCronExpression();

                if (entity != null)
                {
                    entity.Schedule = cron;
                    entity.Name = name;
                }
                else
                {
                    entity = new Database.ScheduleGroup
                    {
                        Schedule = cron,
                        Name = cron
                    };
                    db.ScheduleGroups.Add(entity);
                }

                db.SaveChanges();
                return entity.GroupId;
            }
        }

        public void Delete(long groupId)
        {
            using (var db = new GatewayEntities())
            {
                var entity = db.ScheduleGroups.SingleOrDefault(x => x.GroupId == groupId);

                if (entity == null)
                {
                    throw new Exception($"Unable to find group with ID {groupId}");
                }

                var schedules = entity.Schedules.ToList();
                foreach (var schedule in schedules)
                {
                    _scheduleDataService.DeleteSchedule(schedule.ScheduleId, db, false);
                }

                db.ScheduleGroups.Remove(entity);
                db.SaveChanges();
            }
        }

        private Func<Database.Schedule, bool> GetSearchCriteria(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return x => true;

            var terms = s.ToLower().Split(' ');
            return x => terms.Any(y => (x.TradeSource != null && x.TradeSource.ToLower().Contains(y)) ||
                                       (x.RiskBatchConfiguration != null && x.RiskBatchConfiguration.Type.ToLower().Contains(y)));
        }
    }
}