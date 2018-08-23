using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Batches.Interfaces;

namespace Gateway.Web.Services.Schedule.Utils
{
    public static class ScheduleBatchModelEx
    {
        public static ScheduleBatchModel ToBatchInputModel(this Database.Schedule schedule)
        {
            return new ScheduleBatchModel
            {
                ScheduleId = schedule.ScheduleId,
                ScheduleKey = schedule.ScheduleKey,
                ConfigurationIdList = new List<string> { schedule.RiskBatchConfigurationId.ToString() },
                TradeSources = schedule.TradeSource,
                Group = schedule.GroupId.ToString(),
                Parent = schedule.Parent?.ToString(),
                Children = schedule.Children.Select(x => x.ScheduleId.ToString()).ToList(),
                DisplayName = schedule.Name
            };
        }

        public static ScheduleBatchModel ToBatchInputModel(this IList<Database.Schedule> schedules)
        {
            return new ScheduleBatchModel
            {
                ScheduleId = 0,
                ScheduleKey = string.Empty,
                ConfigurationIdList = schedules.Select(x => x.RiskBatchConfigurationId.ToString()).ToList(),
                TradeSources = string.Join(",",schedules.Select(x => x.TradeSource).ToList()),
                Group = string.Empty,
                Parent = string.Empty,
                Children = new List<string>(),
                DisplayName = string.Empty
            };
        }


        public static void SetData(this ScheduleBatchModel model, IBatchConfigDataService service)
        {
            model.SetGroups(service, new[] { model.Group });
            model.SetParents(service, new[] { model.Parent });
            model.SetChildren(service, model.Children.ToArray());
            model.SetConfigurationTypes(service, model.ConfigurationIdList.ToArray());
        }

        public static void SetConfigurationTypes(this ScheduleBatchModel model, IBatchConfigDataService service, string[] selectedValues)
        {
            model.Types = service
                .GetConfigurations()
                .Select(x => new SelectListItem { Value = x.ConfigurationId.ToString(), Text = x.Type })
                .ToList();

            var selectListItems = model.Types.Where(x => selectedValues.Contains(x.Value)).ToList();
            selectListItems.ForEach(x => x.Selected = true);
        }
    }
}