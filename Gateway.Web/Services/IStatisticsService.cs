using System;
using System.Collections.Generic;

namespace Gateway.Web.Services
{
    public interface IStatisticsService
    {
        List<SummaryStatistic> GetChildMessageSummary(Guid correlationId);
    }
}