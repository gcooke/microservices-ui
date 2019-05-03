using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Absa.Cib.MIT.TaskScheduling.Models;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Schedule.Models;

namespace Gateway.Web.ModelBindersConverters
{
    public class ScheduleBatchModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var request = controllerContext.HttpContext.Request;
            var configurationIdList = request.Form["ConfigurationIdList"] == null ? 
                new List<string>() : 
                request.Form["ConfigurationIdList"].Split(',').ToList();
            var tradeSourceType = request.Form["TradeSourceType"];

            var model = new ScheduleBatchModel
            {
                ScheduleId = long.Parse(request.Form["ScheduleId"]),
                Group = request.Form["Group"],
                Parent = request.Form["Parent"],
                GroupName = request.Form["GroupName"],
                BulkUpdate = bool.Parse(request.Form["BulkUpdate"]),
                ConfigurationIdList = configurationIdList
            };

            foreach (var index in Enumerable.Range(0, model.TradeSources.Count))
            {
                var tradeSource = request.Form[$"TradeSources[{index}].TradeSource"];
                var site = request.Form[$"TradeSources[{index}].Site"];
                var marketDataMap = request.Form[$"TradeSources[{index}].MarketDataMap"];
                var isLive = request.Form[$"TradeSources[{index}].IsLive"]?.Split(',')[0];
                if (string.IsNullOrWhiteSpace(isLive)) isLive = "false";

                bool isLiveValue;
                if (!bool.TryParse(isLive, out isLiveValue))
                {
                    throw new Exception("Unable to parse value for 'Is Live'");
                }

                model.TradeSources.Add(new TradeSourceParameter(tradeSourceType, tradeSource, site, isLiveValue)
                {
                    MarketDataMap = marketDataMap
                });
            }

            foreach (var index in Enumerable.Range(0, model.Properties.Count))
            {
                var key = request.Form[$"Properties[{index}].Key"];
                var value = request.Form[$"Properties[{index}].Value"];
                model.Properties.Add(new Header(key, value));
            }

            model.Properties = model.Properties.Where(x => !string.IsNullOrWhiteSpace(x.Key)).ToList();
            model.TradeSources = model.TradeSources.Where(x => !x.IsEmpty()).ToList();

            return model;
        }
    }
}