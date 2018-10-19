using System;
using Gateway.Web.Database;
using Gateway.Web.Models.Schedule.Input;

namespace Gateway.Web.Services.Schedule.Models
{
    public class ExecutableScheduleParameters : BaseScheduleParameters
    {
        public string Name { get; set; }

        public string PathToExe { get; set; }

        public string Arguments { get; set; }

        public long? ExecutableConfigurationId { get; set; }

        public override void Populate(GatewayEntities db, BaseScheduleModel model)
        {
            base.Populate(db, model);

            var m = model as ScheduleExecutableModel;

            if (m == null)
                throw new Exception($"Model is not of type {typeof(ScheduleBatchModel).Name}");

            PathToExe = m.PathToExe;
            Arguments = m.Arguments;
            Name = m.Name;
            ExecutableConfigurationId = m.ExecutableConfigurationId;
        }
    }
}