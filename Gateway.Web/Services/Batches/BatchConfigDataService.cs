using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Bagl.Cib.MIT.IoC;
using Gateway.Web.Database;
using Gateway.Web.Services.Batches.Interfaces;

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
                    .ToList();
                return items;
            }
        }

        public IList<Database.Schedule> GetSchedules()
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var items = db.Schedules
                    .Include("RiskBatchConfiguration")
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
