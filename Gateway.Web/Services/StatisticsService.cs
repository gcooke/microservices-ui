using System;
using System.Collections.Generic;
using System.Linq;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Database;

namespace Gateway.Web.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ILogger _logger;
        private readonly IGatewayDatabaseService _gatewayDatabaseService;

        public StatisticsService(ILoggingService loggingService,
            IGatewayDatabaseService gatewayDatabaseService)
        {
            _logger = loggingService.GetLogger(this);
            _gatewayDatabaseService = gatewayDatabaseService;
        }

        private IEnumerable<SummaryStatistic> GetChildMessageSummaryImpl(IEnumerable<RequestResponsePair> source)
        {
            var controllerSet = source
                .OrderBy(p => p.Request.StartUtc)
                .GroupBy(p => p.Request.Controller)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var set in controllerSet)
            {
                var totalQueueTime = 0;
                var totalProcessingTime = 0;

                foreach (var messagePair in set.Value)
                {
                    var queueTime = messagePair.Response.QueueTimeMs;
                    totalQueueTime += queueTime;
                    totalProcessingTime += messagePair.Response.TimeTakeMs - queueTime;
                }

                var result = new SummaryStatistic()
                {
                    Controller = set.Key,
                    CallCount = set.Value.Count,
                    TotalQueueTime = TimeSpan.FromMilliseconds(totalQueueTime),
                    TotalProcessingTime = TimeSpan.FromMilliseconds(totalProcessingTime)
                };

                yield return result;
            }
        }

        public List<SummaryStatistic> GetChildMessageSummary(Guid correlationId)
        {
            var messages = _gatewayDatabaseService.GetChildMessagePairs(correlationId);

            var result = GetChildMessageSummaryImpl(messages)
                .OrderBy(s => s.Controller)
                .ToList();

            return result;
        }
    }

    public class SummaryStatistic
    {
        public string Controller { get; set; }

        public int CallCount { get; set; }

        public TimeSpan TotalQueueTime { get; set; }

        public TimeSpan TotalProcessingTime { get; set; }
    }
}