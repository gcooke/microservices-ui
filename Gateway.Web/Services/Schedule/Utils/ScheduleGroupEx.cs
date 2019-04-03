using Gateway.Web.Models.Schedule;
using Gateway.Web.Models.Schedule.Output;
using Gateway.Web.Utils;

namespace Gateway.Web.Services.Schedule.Utils
{
    public static class ScheduleGroupEx
    {
        public static ScheduleGroup ToModel(this Database.ScheduleGroup entity)
        {
            return new ScheduleGroup
            {
                GroupId = entity.GroupId,
                Schedule = CronTabExpression.Parse(entity.Schedule).FromCronExpression(),
            };
        }
    }
}