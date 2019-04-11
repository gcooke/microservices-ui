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
            var startUtc = start.ToUniversalTime();
            var endUtc = end.ToUniversalTime();
            var requests = db.Requests
                .Where(x => x.Resource.Contains(schedule.ScheduleId.ToString()))
                .Where(x => x.StartUtc >= startUtc && x.StartUtc <= endUtc)
                .Where(x => x.Controller.ToLower() == "riskbatch")
                .ToList();

            var requestIdList = requests.Select(y => y.CorrelationId).ToList();
            var responses = db.Responses
                .Where(x => requestIdList.Contains(x.CorrelationId))
                .ToList();

            var batch = new Batch();

            batch.BatchType = schedule.RiskBatchSchedule.RiskBatchConfiguration.Type;
            batch.TradeSource = schedule.RiskBatchSchedule.TradeSource;
            batch.Site = schedule.RiskBatchSchedule.Site;
            batch.TradeSourceType = schedule.RiskBatchSchedule.TradeSourceType;
            batch.ExpectedOccurrences = schedule.ScheduleGroup.Schedule.GetCronOccurrencesBetween(start, end);

            foreach (var request in requests)
            {
                var response = responses.SingleOrDefault(x => x.CorrelationId == request.CorrelationId);
                var batchRun = new BatchRun();
                batchRun.ValuationDate = GetBusinessDate(request?.Resource);
                batchRun.CorrelationId = request?.CorrelationId;
                batchRun.StartedAt = request?.StartUtc.ToLocalTime();
                batchRun.FinishedAt = response?.EndUtc.ToLocalTime();
                batchRun.CurrentStatus = GetBatchStatus(response);
                batch.ActualOccurrences.Add(batchRun);
            }

            return batch;
        }

        private static string GetBatchStatus(Response response)
        {
            if (response == null) return "Executing task...";
            if (response.ResultCode == 1) return "Succeeded";
            else return "Failed";
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