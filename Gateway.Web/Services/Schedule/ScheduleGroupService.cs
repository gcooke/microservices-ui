using Bagl.Cib.MIT.IoC;
using Gateway.Web.Database;
using Gateway.Web.Services.Schedule.Interfaces;
using Gateway.Web.Services.Schedule.Utils;
using Gateway.Web.Utils;
using NCrontab;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Gateway.Web.Models.Schedule.Output;
using ScheduleGroup = Gateway.Web.Models.Schedule.Output.ScheduleGroup;

namespace Gateway.Web.Services.Schedule
{
    public class ScheduleGroupService : IScheduleGroupService
    {
        private readonly IScheduleDataService _scheduleDataService;
        public readonly string ConnectionString;

        public ScheduleGroupService(IScheduleDataService scheduleDataService, ISystemInformation systemInformation)
        {
            _scheduleDataService = scheduleDataService;
            ConnectionString = systemInformation.GetConnectionString("GatewayDatabase", "Database.PnRFO_Gateway");
        }

        public IList<ScheduleGroup> GetScheduleGroups(DateTime startDate, DateTime endDate, string searchTerm = null,
            bool includeAllGroups = false, bool includeDisabledTasks = true)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                string status = null;
                if (!string.IsNullOrWhiteSpace(searchTerm) && searchTerm.Contains("status:"))
                {
                    var statusIndex = searchTerm.IndexOf("status:", StringComparison.Ordinal);
                    status = searchTerm.Substring(statusIndex + "status:".Length).Trim();
                    searchTerm = statusIndex > 0 ? searchTerm.Substring(0, statusIndex - 1).Trim() : string.Empty;
                }

                var groups = GetGroups(startDate, endDate, searchTerm);
                var runGroups = new List<ScheduleGroup>();

                foreach (var group in groups)
                {
                    group.FriendScheduledTime = group.NextOccurrence.ToString("hh:mm tt");

                    if (!includeDisabledTasks)
                    {
                        group.Tasks = group.Tasks.Where(x => x.IsEnabled).ToList();
                    }

                    var tasks = new List<ScheduleTask>();
                    foreach (var task in group.Tasks)
                    {
                        var jobList = db.ScheduledJobs
                            .Where(x => x.ScheduleId == task.ScheduleId)
                            .Where(x => x.BusinessDate > startDate && x.BusinessDate < endDate)
                            .ToList();

                        var lastJob = jobList.LastOrDefault();

                        if (!string.IsNullOrWhiteSpace(status))
                        {
                            if (lastJob == null)
                                continue;

                            if (lastJob.Status.ToLower() != status.ToLower())
                                continue;
                        }

                        if (lastJob == null)
                        {
                            task.Status = "Not Started";
                            tasks.Add(task);
                            continue;
                        }

                        task.Status = lastJob.Status;
                        task.Retries = jobList.Count - 1;
                        task.RequestId = lastJob.RequestId;
                        task.StartedAt = lastJob.StartedAt;
                        task.FinishedAt = lastJob.FinishedAt;
                        task.BusinessDate = lastJob.BusinessDate;
                        tasks.Add(task);
                    }

                    group.Tasks = tasks;
                    runGroups.Add(group);
                }

                return runGroups.OrderBy(x => x.NextOccurrence).ToList();
            }
        }


        public IList<ScheduleGroup> GetGroups(DateTime startDate, DateTime endDate, string searchTerm = null)
        {
            using (var db = new GatewayEntities(ConnectionString))
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

                    var crontabSchedule = CrontabSchedule.Parse(group.Schedule);
                    var occurrences = crontabSchedule
                        .GetNextOccurrences(startDate, endDate)
                        .ToList();

                    if (!occurrences.Any())
                    {
                        groupModels.Remove(groupModel);
                        continue;
                    }

                    groupModel.NextOccurrence = occurrences.First();

                    foreach (var schedule in rootChildren)
                    {
                        groupModel.Tasks.Add(schedule.ToModel());

                        if (schedule.Children.Any())
                        {
                            AddChildren(schedule, groupModel, searchTerm);
                        }
                    }
                }

                return groupModels.OrderBy(x => x.NextOccurrence.TimeOfDay).ToList();
            }
        }

        public IList<ScheduleGroup> GetGroups(IList<long> idList)
        {
            using (var db = new GatewayEntities(ConnectionString))
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
            using (var db = new GatewayEntities(ConnectionString))
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
            using (var db = new GatewayEntities(ConnectionString))
            {
                var entity = db.ScheduleGroups.SingleOrDefault(x => x.Schedule == cron);

                if(name == null)
                    name = CronExpressionDescriptor.ExpressionDescriptor.GetDescription(cron);

                cron = CronTabExpression.Parse(cron).ToString();

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
            using (var db = new GatewayEntities(ConnectionString))
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
            return x => terms.Any(y => (x.RiskBatchSchedule != null && x.RiskBatchSchedule.TradeSource != null && x.RiskBatchSchedule.TradeSource.ToLower().Contains(y)) ||
                                       (x.RiskBatchSchedule != null && x.RiskBatchSchedule.RiskBatchConfiguration.Type.ToLower().Contains(y)) ||
                                       (x.RequestConfiguration != null && x.RequestConfiguration.Name.ToLower().Contains(y)));
        }
    }
}