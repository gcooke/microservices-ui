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
        private readonly IGatewayService _gatewayService;

        public StatisticsService(ILoggingService loggingService,
            IGatewayDatabaseService gatewayDatabaseService,
            IGatewayService gatewayService)
        {
            _logger = loggingService.GetLogger(this);
            _gatewayDatabaseService = gatewayDatabaseService;
            _gatewayService = gatewayService;
        }

        private void GetFullChildData(Guid correlationId)
        {
            var data = _gatewayDatabaseService.GetChildMessages(correlationId, r => r).ToList();
        }

        public Timings GetTimings(Guid correlationId)
        {
            MessageHierarchy payload = null;
            GetFullChildData(correlationId);
            var result = new Timings(payload);
            return result;
        }
    }
}
