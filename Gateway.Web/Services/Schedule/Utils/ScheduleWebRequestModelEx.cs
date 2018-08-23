using System.Collections.Generic;
using System.Linq;
using Absa.Cib.MIT.TaskScheduling.Models;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Batches.Interfaces;
using Newtonsoft.Json;

namespace Gateway.Web.Services.Schedule.Utils
{
    public static class ScheduleWebRequestModelEx
    {
        public static ScheduleWebRequestModel ToReqeustInputModel(this Database.Schedule schedule)
        {
            var model = new ScheduleWebRequestModel
            {
                ScheduleId = schedule.ScheduleId,
                ScheduleKey = schedule.ScheduleKey,
                Group = schedule.GroupId.ToString(),
                Parent = schedule.Parent?.ToString(),
                Children = schedule.Children.Select(x => x.ScheduleId.ToString()).ToList(),
                DisplayName = schedule.Name,
                Name = schedule.RequestConfiguration?.Name,
                Url = schedule.RequestConfiguration?.Url,
                Verb = schedule.RequestConfiguration?.Verb,
                Payload = schedule.RequestConfiguration?.Payload,
                RequestConfigurationId = schedule.RequestConfigurationId
            };

            var arguments = JsonConvert.DeserializeObject<List<Argument>>(schedule.RequestConfiguration?.Arguments);
            var headers = JsonConvert.DeserializeObject<List<Header>>(schedule.RequestConfiguration?.Headers);

            foreach (var index in Enumerable.Range(0, arguments.Count))
            {
                model.Arguments[index] = arguments[index];
            }

            foreach (var index in Enumerable.Range(0, headers.Count))
            {
                model.Headers[index] = headers[index];
            }

            return model;
        }

        public static void SetData(this ScheduleWebRequestModel model, IBatchConfigDataService service)
        {
            model.SetGroups(service, new[] { model.Group });
            model.SetParents(service, new[] { model.Parent });
            model.SetChildren(service, model.Children.ToArray());
        }
    }
}