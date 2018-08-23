using System.Collections.Generic;
using System.Linq;
using Gateway.Web.Database;
using Gateway.Web.Services.Batches.Interfaces;

namespace Gateway.Web.Services.Batches
{
    public class BatchConfigDataService : IBatchConfigDataService
    {
        public IList<ScheduleGroup> GetGroups()
        {
            using (var db = new GatewayEntities())
            {
                var items = db.ScheduleGroups
                    .ToList();
                return items;
            }
        }

        public IList<RiskBatchConfiguration> GetConfigurations()
        {
            using (var db = new GatewayEntities())
            {
                var items = db.RiskBatchConfigurations
                    .ToList();
                return items;
            }
        }

        public IList<Database.Schedule> GetSchedules()
        {
            using (var db = new GatewayEntities())
            {
                var items = db.Schedules
                    .Include("RiskBatchConfiguration")
                    .Include("RequestConfiguration")
                    .Include("ParentSchedule")
                    .Include("Children")
                    .ToList();
                return items;
            }
        }
    }
}
