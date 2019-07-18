using System;
using System.Collections.Generic;
using Gateway.Web.Models.Request;

namespace Gateway.Web.Services
{
    public interface IStatisticsService
    {
        Timings GetTimings(Guid correlationId);
    }
}