using System;
using System.Collections.Generic;
using System.Linq;
using Bagl.Cib.MIT.IoC;
using Gateway.Web.Database;
using Gateway.Web.Services.Batches.Interrogation.Models;
using Gateway.Web.Services.Batches.Interrogation.Services.MonitoringWriterService;
using Gateway.Web.Services.Batches.Interrogation.Utils;

namespace Gateway.Web.Services.Batches.Interrogation.Services.BatchService
{
    public class BatchService : IBatchService
    {
        private readonly IMonitoringWriterService _writer;
        private readonly ISystemInformation _information;
        private readonly string _connectionString;

        public BatchService(IMonitoringWriterService writer, ISystemInformation information)
        {
            _writer = writer;
            _information = information;
            _connectionString = information.GetConnectionString("GatewayDatabase", "Database.PnRFO_Gateway");
        }


        public IEnumerable<Batch> GetBatchesForDate(DateTime date)
        {
            using (var db = new GatewayEntities(_connectionString))
            {
                _writer.WriteText($"Getting all batches...");

                var batches = new List<Batch>();
                var schedules = db
                    .Schedules
                    .Where(x => x.RiskBatchScheduleId != null)
                    .Where(x => x.IsEnabled != false)
                    .ToList();

                _writer.WriteText($"Determining which batch will be on {date:yyyy-MM-dd} ...");

                var startDate = date.Date;
                var endDate = startDate.AddHours(23).AddMinutes(59).AddSeconds(59);

                _writer.WriteText($"Obtaining run information for batches that will be run {date:yyyy-MM-dd} ...");
                foreach (var schedule in schedules)
                {
                    var cron = schedule.ScheduleGroup.Schedule;
                    if (!cron.WillCronTriggerBetween(startDate, endDate)) continue;
                    var batch = BatchFactory.Create(db, schedule, startDate, endDate);
                    batches.Add(batch);
                }

                _writer.WriteText($"Retrieved all batch information for {date:yyyy-MM-dd}.");
                _writer.WriteLine();

                return batches;
            }
        }
    }
}