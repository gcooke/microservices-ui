using System;
using System.Collections.Generic;
using System.Linq;
using Absa.Cib.MIT.TaskScheduling.Models;
using Absa.Cib.MIT.TaskScheduling.Models.Builders.RedstoneRequest;
using Absa.Cib.MIT.TaskScheduling.Models.Enum;
using Bagl.Cib.MSF.Contracts.Compression;
using Newtonsoft.Json;
using RestSharp;

namespace Gateway.Web.Services.Schedule.Utils
{
    public static class BatchRequestBuilderEx
    {
        private static readonly string AuthUrl = System.Configuration.ConfigurationManager.AppSettings["AuthUrl"];
        private static readonly string BaseUrl = System.Configuration.ConfigurationManager.AppSettings["BaseUrl"];
        private static readonly string AuthQuery = System.Configuration.ConfigurationManager.AppSettings["AuthQuery"];
        private static string _query = "RiskBatch/Official/Batch/Run/%id%/%valuationDate%";

        public static RedstoneRequest ToRequest(this Database.Schedule schedule, DateTime? businessDate = null)
        {
            if (schedule.RiskBatchConfigurationId.HasValue)
            {
                var batchRequest = RedstoneRequestBuilder
                    .Create()
                    .SetBaseUrl(BaseUrl)
                    .SetAuthUrl(AuthUrl)
                    .SetQuery(_query)
                    .SetAuthQuery(AuthQuery)
                    .SetMethod(Method.PUT)
                    .SetId(schedule.ScheduleId)
                    .SetBusinessDate(businessDate)
                    .Build();

                batchRequest.Arguments.Add(new Argument("id", ArgumentDataTypes.String.ToString(), schedule.ScheduleId.ToString()));
                batchRequest.Arguments.Add(new Argument("valuationDate", ArgumentDataTypes.PreviousWeekDay.ToString(), "yyyy-MM-dd"));

                AddChildRequest(batchRequest, schedule.Children.ToList());
                return batchRequest;
            }

            var webRequest = RedstoneRequestBuilder
                .Create()
                .SetBaseUrl(schedule.RequestConfiguration.Url)
                .SetAuthUrl(AuthUrl)
                .SetQuery("")
                .SetAuthQuery(AuthQuery)
                .SetMethod((Method) Enum.Parse(typeof(Method), schedule.RequestConfiguration.Verb.ToUpper()))
                .SetId(schedule.ScheduleId)
                .SetBusinessDate(businessDate)
                .SetBody(schedule.RequestConfiguration.Payload)
                .Build();

            var headers = JsonConvert.DeserializeObject<IList<Header>>(schedule.RequestConfiguration.Headers);
            foreach (var header in headers)
            {
                webRequest.AddHeader(new Header(header.Key, header.Value));
            }

            var arguments = JsonConvert.DeserializeObject<IList<Argument>>(schedule.RequestConfiguration.Arguments);
            foreach (var argument in arguments)
            {
                webRequest.AddArgument(new Argument(argument.Key, argument.Type, argument.FormatValue));
            }

            return webRequest;
        }

        private static void AddChildRequest(RedstoneRequest parent, IList<Database.Schedule> children)
        {
            if (!children.Any())
                return;

            foreach (var child in children)
            {
                var request = child.ToRequest();
                parent.ContinueWith.Add(request);
                AddChildRequest(request, child.Children.ToList());
            }
        }
    }
}