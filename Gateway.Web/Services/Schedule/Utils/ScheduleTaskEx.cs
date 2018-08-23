using System;
using Gateway.Web.Models.Schedule.Output;

namespace Gateway.Web.Services.Schedule.Utils
{
    public static class ScheduleTaskEx
    {
        public static ScheduleTask ToModel(this Database.Schedule schedule)
        {
            var task = new ScheduleTask(schedule.Type)
            {
                ScheduleId = schedule.ScheduleId,
                Schedule = string.Empty,
                ScheduleKey = schedule.ScheduleKey,
                ParentId = schedule.Parent,
                Parent = "",
                ChildrenCount = schedule.Children.Count,
                GroupName = schedule.ScheduleGroup?.Schedule,
                Name = schedule.Name,
                Key = schedule.Key
            };

            return task;
        }
    }
}