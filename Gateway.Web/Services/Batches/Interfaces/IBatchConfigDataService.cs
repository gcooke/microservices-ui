using System.Collections.Generic;
using Gateway.Web.Database;

namespace Gateway.Web.Services.Batches.Interfaces
{
    public interface IBatchConfigDataService
    {
        IList<ScheduleGroup> GetGroups();
        IList<RiskBatchConfiguration> GetConfigurations();
        IList<Database.Schedule> GetSchedules();
    }
}
