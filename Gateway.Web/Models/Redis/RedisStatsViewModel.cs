using System.Collections.Generic;

namespace Gateway.Web.Models.Redis
{
    public class RedisStatsViewModel
    {
        public string ControllerName { get; set; }
        public string ControllerVersion { get; set; }
        public List<RedisStats> RedisStats { get; set; }
    }
}