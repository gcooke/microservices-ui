using Gateway.Web.Models.Redis;
using System.Collections.Generic;

namespace Gateway.Web.Services
{
    public interface IRedisService
    {
        IList<RedisStats> GetRedisStats(string controllerName, string controlerversion, int maxPriority);

        RedisSummary GetRedisSummary(string controllerName, string controlerversion, int maxPriority);
    }
}