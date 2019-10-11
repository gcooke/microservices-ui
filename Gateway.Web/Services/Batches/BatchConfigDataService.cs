using Bagl.Cib.MIT.IoC;
using Gateway.Web.Database;
using Gateway.Web.Services.Batches.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Services.Batches
{
    public class BatchConfigDataService : IBatchConfigDataService
    {
        public readonly string ConnectionString = String.Empty;

        public BatchConfigDataService(ISystemInformation systemInformation)
        {
            ConnectionString = systemInformation.GetConnectionString("GatewayDatabase", "Database.PnRFO_Gateway");
        }

        public IList<ScheduleGroup> GetGroups()
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var items = db.ScheduleGroups
                    .ToList();
                return items;
            }
        }

        public IList<RiskBatchConfiguration> GetConfigurations()
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var items = db.RiskBatchConfigurations
                    .OrderBy(i => i.Type)
                    .ToList();
                return items;
            }
        }

        public IList<Database.Schedule> GetSchedules()
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var items = db.Schedules
                    .Include("RiskBatchSchedule")
                    .Include("RiskBatchSchedule.RiskBatchConfiguration")
                    .Include("ExecutableConfiguration")
                    .Include("RequestConfiguration")
                    .Include("ParentSchedule")
                    .Include("Children")
                    .ToList();
                return items;
            }
        }
    }
}