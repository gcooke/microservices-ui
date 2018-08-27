using System;
using System.Collections.Generic;
using System.Linq;
using Absa.Cib.MIT.TaskScheduling.Models;
using Gateway.Web.Database;
using Gateway.Web.Models.Schedule.Input;

namespace Gateway.Web.Services.Schedule.Models
{
    public class RedstoneRequestParameters : BaseScheduleParameters
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string Verb { get; set; }

        public string Payload { get; set; }

        public string GroupId { get; set; }

        public IList<Argument> Arguments { get; set; }

        public IList<Header> Headers { get; set; }

        public long? RequestConfigurationId { get; set; }

        public override void Populate(GatewayEntities db, BaseScheduleModel model)
        {
            base.Populate(db, model);

            var m = model as ScheduleWebRequestModel;

            if (m == null)
                throw new Exception($"Model is not of type {typeof(ScheduleWebRequestModel).Name}");

            Name = m.Name;
            Url = m.Url;
            Verb = m.Verb;
            Payload = m.Payload;
            Arguments = m.Arguments;
            RequestConfigurationId = m.RequestConfigurationId;
            IsAsync = m.Headers.Any(x => x.Key != null && x.Value != null && x.Key.ToLower() == "isasync" && x.Value.ToLower() == "true");
            Headers = m.Headers;
        }
    }
}