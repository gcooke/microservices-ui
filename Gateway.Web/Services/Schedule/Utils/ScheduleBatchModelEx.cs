using Absa.Cib.MIT.TaskScheduling.Models;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Batches.Interfaces;
using Gateway.Web.Services.Schedule.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Gateway.Web.Services.Schedule.Utils
{
    public static class ScheduleBatchModelEx
    {
        public static ScheduleBatchModel ToBatchInputModel(this Database.Schedule schedule)
        {
            var model = new ScheduleBatchModel
            {
                ScheduleId = schedule.ScheduleId,
                ScheduleKey = schedule.ScheduleKey,
                ConfigurationIdList = new List<string> { schedule.RiskBatchSchedule?.RiskBatchConfigurationId.ToString() },
                Group = schedule.GroupId.ToString(),
                Parent = schedule.Parent?.ToString(),
                Children = schedule.Children.Select(x => x.ScheduleId.ToString()).ToList(),
                DisplayName = schedule.Name,
                TradeSourceType = schedule.RiskBatchSchedule?.TradeSourceType
            };

            var properties = JsonConvert
                .DeserializeObject<Dictionary<string, string>>(schedule.RiskBatchSchedule?.AdditionalProperties)
                .Select(x => new Header(x.Key, x.Value))
                .ToList();

            var tradeSources = new List<TradeSourceParameter>
            {
                new TradeSourceParameter(schedule.RiskBatchSchedule?.TradeSourceType, schedule.RiskBatchSchedule?.TradeSource, schedule.RiskBatchSchedule?.Site, schedule.RiskBatchSchedule?.IsLive ?? false, schedule.RiskBatchSchedule?.IsT0 ?? false)
                {
                    MarketDataMap = schedule.RiskBatchSchedule?.MarketDataMap
                }
            };

            foreach (var index in Enumerable.Range(0, properties.Count))
            {
                model.Properties[index] = properties[index];
            }

            foreach (var index in Enumerable.Range(0, tradeSources.Count))
            {
                model.TradeSources[index] = tradeSources[index];
            }

            return model;
        }

        public static ScheduleBatchModel ToBatchInputModel(this IList<Database.Schedule> schedules)
        {
            var model = new ScheduleBatchModel
            {
                ScheduleId = 0,
                ScheduleKey = string.Empty,
                ConfigurationIdList = schedules.Select(x => x.RiskBatchSchedule?.RiskBatchConfigurationId.ToString()).ToList(),
                Group = string.Empty,
                Parent = string.Empty,
                Children = new List<string>(),
                DisplayName = string.Empty,
                TradeSourceType = schedules.FirstOrDefault()?.RiskBatchSchedule?.TradeSourceType
            };

            var tradeSources = schedules
                .Select(schedule => new TradeSourceParameter(schedule.RiskBatchSchedule?.TradeSourceType, schedule.RiskBatchSchedule?.TradeSource, schedule.RiskBatchSchedule?.Site, schedule.RiskBatchSchedule?.IsLive ?? false, schedule.RiskBatchSchedule?.IsT0 ?? false) { MarketDataMap = schedule.RiskBatchSchedule?.MarketDataMap })
                .Distinct()
                .ToList();

            foreach (var index in Enumerable.Range(0, tradeSources.Count))
            {
                model.TradeSources[index] = tradeSources[index];
            }

            var properties = schedules
                .SelectMany(x => JsonConvert.DeserializeObject<IDictionary<string, string>>(x.RiskBatchSchedule?.AdditionalProperties))
                .Select(x => new Header(x.Key, x.Value))
                .Distinct(new HeaderEqualityComparer())
                .ToList();

            foreach (var index in Enumerable.Range(0, properties.Count))
            {
                model.Properties[index] = properties[index];
            }

            return model;
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
                .Select(x => new SelectListItem { Value = x.ConfigurationId.ToString(), Text = $"{x.Type} (OutputTag : {x.OutputTag})" })
                .ToList();

            var selectListItems = model.Types.Where(x => selectedValues.Contains(x.Value)).ToList();
            selectListItems.ForEach(x => x.Selected = true);
        }
    }

    public class HeaderEqualityComparer : IEqualityComparer<Header>
    {
        public bool Equals(Header x, Header y)
        {
            return x != null && y != null && x.Key == y.Key;
        }

        public int GetHashCode(Header obj)
        {
            return $"{obj.Key}{obj.Value}".GetHashCode();
        }
    }
}