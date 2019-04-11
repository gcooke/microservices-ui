using System;
using System.Collections.Generic;
using System.Linq;
using Bagl.Cib.MIT.IoC;
using Gateway.Web.Database;
using Gateway.Web.Models.Interrogation;
using Gateway.Web.Services.Batches.Interrogation.Models;
using Gateway.Web.Services.Batches.Interrogation.Utils;

namespace Gateway.Web.Services.Batches.Interrogation.Services.BatchService
{
    public class BatchService : IBatchService
    {
        private readonly string _connectionString;

        public BatchService(ISystemInformation information)
        {
            _connectionString = information.GetConnectionString("GatewayDatabase", "Database.PnRFO_Gateway");
        }

        public IEnumerable<Batch> GetBatchesForDate(InterrogationModel model)
        {
            var list = new List<Batch>();
            using (var db = new GatewayEntities(_connectionString))
            {
                // Getting all batches
                var schedules = db
                    .Schedules
                    .Where(x => x.RiskBatchScheduleId != null)
                    .Where(x => x.IsEnabled != false)
                    .ToList();

                var startDate = model.ReportDate.Date;
                var endDate = startDate.AddHours(23).AddMinutes(59).AddSeconds(59);

                foreach (var schedule in schedules)
                {
                    // Check if batch is relevant for period
                    var cron = schedule.ScheduleGroup.Schedule;
                    if (!cron.WillCronTriggerBetween(startDate, endDate)) continue;

                    // Check if batch matches parameters
                    if (!IsMatch(schedule, model)) continue;

                    var batch = BatchFactory.Create(db, schedule, startDate, endDate);
                    list.Add(batch);
                }
            }
            return list;
        }

        private bool IsMatch(Database.Schedule schedule, InterrogationModel model)
        {
            if (!string.Equals(schedule.RiskBatchSchedule.TradeSource, model.TradeSource, StringComparison.CurrentCultureIgnoreCase))
                return false;
            if (!string.Equals(schedule.RiskBatchSchedule.RiskBatchConfiguration.Type, model.BatchType, StringComparison.CurrentCultureIgnoreCase))
                return false;
            return true;
        }
    }
}