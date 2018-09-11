using System;
using System.Collections.Generic;
using Gateway.Web.Database;
using Gateway.Web.Models.Schedule;
using Gateway.Web.Models.Schedule.Output;
using ScheduleGroup = Gateway.Web.Models.Schedule.Output.ScheduleGroup;

namespace Gateway.Web.Services.Schedule.Interfaces
{
    public interface IScheduleDataService
    {
        IList<ScheduleTask> GetScheduleTasks(IEnumerable<long> scheduleIdList);
        ScheduleTask GetScheduleTask(long id);
        Database.Schedule GetSchedule(long id);
        IList<Database.Schedule> GetSchedules(IEnumerable<long> scheduleIdList);
        IDictionary<string, string> GetDailyStatuses(DateTime now);
        void DeleteSchedule(long id, GatewayEntities db = null, bool saveChanges = true);
        void DeleteForConfiguration(long id, GatewayEntities db = null, bool saveChanges = true);
        void RerunTask(long id, DateTime businessDate);
        void RerunTaskGroup(long id, DateTime businessDate, string searchTerm);

    }
}
