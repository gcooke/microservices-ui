using System;
using System.Collections.Generic;
using Gateway.Web.Models.Request;

namespace Gateway.Web.Services
{
    public interface IStatisticsService
    {
        List<SummaryStatistic> GetChildMessageSummary(Guid correlationId);

        Timings GetTimings(Guid correlationId);
    }
}