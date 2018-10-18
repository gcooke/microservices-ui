using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Batches.Interfaces;

namespace Gateway.Web.Services.Schedule.Utils
{
    public static class ScheduleExecutableModelEx
    {
        public static ScheduleExecutableModel ToExecutableInputModel(this Database.Schedule schedule)
        {
            return new ScheduleExecutableModel()
            {
                ScheduleId = schedule.ScheduleId,
                ScheduleKey = schedule.ScheduleKey,
                Group = schedule.GroupId.ToString(),
                Parent = schedule.Parent?.ToString(),
                Children = schedule.Children.Select(x => x.ScheduleId.ToString()).ToList(),
                DisplayName = schedule.Name,
                Name = schedule.ExecutableConfiguration?.Name,
                PathToExe = schedule.ExecutableConfiguration?.ExecutablePath,
                Arguments = schedule.ExecutableConfiguration?.CommandLineArguments,
                ExecutableConfigurationId = schedule.ExecutableConfigurationId
            };
        }

        public static void SetData(this ScheduleExecutableModel model, IBatchConfigDataService service)
        {
            model.SetGroups(service, new[] { model.Group });
            model.SetParents(service, new[] { model.Parent });
            model.SetChildren(service, model.Children.ToArray());
        }
    }
}