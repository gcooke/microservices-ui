using System;
using System.Collections.Generic;
using Gateway.Web.Models.Schedule;
using Gateway.Web.Models.Schedule.Output;

namespace Gateway.Web.Services.Schedule.Interfaces
{
    public interface IScheduleGroupService
    {
        IList<ScheduleGroup> GetGroups(string searchTerm = null);
        ScheduleGroup GetGroup(long id);
        IList<ScheduleGroup> GetGroups(IList<long> idList);
        long CreateOrUpdate(string cron, string name = null);
        void Delete(long groupId);
        IList<ScheduleGroup> GetScheduleGroups(DateTime businessDateValue, string searchTerm, bool includeAllGroups = false);
    }
}