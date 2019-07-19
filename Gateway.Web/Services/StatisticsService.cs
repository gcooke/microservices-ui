using System;
using System.Linq;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Database;
using Gateway.Web.Models.Request;

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

        private MessageHierarchy GetFullChildData(Guid correlationId)
        {
            var records = _gatewayDatabaseService.GetChildMessages(correlationId, MessageHierarchyUtils.ToModel).ToList();
            var result = records.ToTree(correlationId);

            return result;
        }

        public Timings GetTimings(Guid correlationId)
        {
            var payload = GetFullChildData(correlationId);
            var result = new Timings(payload);
            return result;
        }
    }
}
