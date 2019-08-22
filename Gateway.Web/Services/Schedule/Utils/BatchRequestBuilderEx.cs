using Absa.Cib.MIT.TaskScheduling.Models;
using Absa.Cib.MIT.TaskScheduling.Models.Builders.RedstoneRequest;
using Absa.Cib.MIT.TaskScheduling.Models.Enum;
using Absa.Cib.MIT.TaskScheduling.Server.Services.Executable;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Services.Schedule.Utils
{
    public static class BatchRequestBuilderEx
    {
        public static string AuthUrl = String.Empty;
        public static string BaseUrl = String.Empty;
        public static string AuthQuery = String.Empty;
        private static string _query = "RiskBatch/Official/Batch/Run/%id%/%valuationDate%";

        public static RedstoneRequest ToRequest(this Database.Schedule schedule, bool isLive, bool isT0, DateTime? businessDate = null, bool includeChildRequests = true)
        {
            if (schedule.ScheduleId == 0)
                throw new Exception("Unable to create request for scheduling - Schedule Id is set to 0.");

            if (schedule.RiskBatchScheduleId.HasValue)
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
                batchRequest.Arguments.Add(isLive || isT0
                    ? new Argument("valuationDate", ArgumentDataTypes.CurrentDateAndTime.ToString(), "yyyy-MM-dd")
                    : new Argument("valuationDate", ArgumentDataTypes.PreviousWeekDay.ToString(), "yyyy-MM-dd"));

                if (includeChildRequests)
                    AddChildRequest(batchRequest, schedule.Children.ToList());

                return batchRequest;
            }

            var webRequest = RedstoneRequestBuilder
                .Create()
                .SetBaseUrl(schedule.RequestConfiguration.Url)
                .SetAuthUrl(AuthUrl)
                .SetQuery("")
                .SetAuthQuery(AuthQuery)
                .SetMethod((Method)Enum.Parse(typeof(Method), schedule.RequestConfiguration.Verb.ToUpper()))
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

            if (includeChildRequests)
                AddChildRequest(webRequest, schedule.Children.ToList());

            return webRequest;
        }

        public static ExecutableOptions ToExecutableOptions(this Database.Schedule schedule)
        {
            var executableOptions = new ExecutableOptions();
            executableOptions.ScheduleId = schedule.ScheduleId;
            executableOptions.PathToExe = schedule.ExecutableConfiguration?.ExecutablePath;
            executableOptions.CommandLineArguments.Add(schedule.ExecutableConfiguration?.CommandLineArguments);
            return executableOptions;
        }

        private static void AddChildRequest(RedstoneRequest parent, IList<Database.Schedule> children)
        {
            if (!children.Any())
                return;

            foreach (var child in children)
            {
                var request = child.ToRequest(child.RiskBatchSchedule?.IsLive ?? false, child.RiskBatchSchedule?.IsT0 ?? false);
                if (parent.ContinueWith.Any(x => x.ScheduleId == request.ScheduleId)) continue;
                parent.ContinueWith.Add(request);
                AddChildRequest(request, child.Children.ToList());
            }
        }
    }
}