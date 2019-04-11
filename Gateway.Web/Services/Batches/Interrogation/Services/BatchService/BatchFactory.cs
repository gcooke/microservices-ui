using System;
using System.Globalization;
using System.Linq;
using Gateway.Web.Database;
using Gateway.Web.Services.Batches.Interrogation.Models;
using Gateway.Web.Services.Batches.Interrogation.Utils;

namespace Gateway.Web.Services.Batches.Interrogation.Services.BatchService
{
    public class BatchFactory
    {
        public static Batch Create(GatewayEntities db, Database.Schedule schedule, DateTime start, DateTime end)
        {
            var runs = db
                .ScheduledJobs
                .Where(x => x.ScheduleId == schedule.ScheduleId)
                .Where(x => x.StartedAt >= start && (x.FinishedAt == null || x.FinishedAt <= end))
                .OrderByDescending(x => x.StartedAt);

            var batch = new Batch();

            batch.BatchType = schedule.RiskBatchSchedule.RiskBatchConfiguration.Type;
            batch.TradeSource = schedule.RiskBatchSchedule.TradeSource;
            batch.Site = schedule.RiskBatchSchedule.Site;
            batch.TradeSourceType = schedule.RiskBatchSchedule.TradeSourceType;
            batch.ExpectedOccurrences = schedule.ScheduleGroup.Schedule.GetCronOccurrencesBetween(start, end);

            foreach (var run in runs)
            {
                var request = db.Requests.SingleOrDefault(x => x.CorrelationId == run.RequestId);
                var batchRun = new BatchRun();
                batchRun.ValuationDate = GetBusinessDate(request?.Resource) ?? run?.BusinessDate.AddDays(-1);
                batchRun.CorrelationId = run?.RequestId;
                batchRun.StartedAt = run?.StartedAt;
                batchRun.FinishedAt = run?.FinishedAt;
                batchRun.CurrentStatus = run?.Status;
                batch.ActualOccurrences.Add(batchRun);
            }

            return batch;
        }

        private static DateTime? GetBusinessDate(string resource)
        {
            if (string.IsNullOrWhiteSpace(resource))
                return null;

            var resourceParts = resource.Split('/');
            if (resourceParts.Length != 4)
                return null;

            DateTime businessDate;
            if (DateTime.TryParseExact(resourceParts[3], "yyyy-MM-dd", CultureInfo.CurrentUICulture, DateTimeStyles.None, out businessDate ))
                return businessDate;

            return null;
        }
    }
}