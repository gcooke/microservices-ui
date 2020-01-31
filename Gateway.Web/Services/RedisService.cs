using Bagl.Cib.MIT.IoC;
using Gateway.Web.Models.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Web.Services
{
    public class RedisService : IRedisService
    {
        public readonly string _redisPassword;
        public readonly string _redisServer;
        private readonly ISystemInformation _information;

        public RedisService(ISystemInformation information)
        {
            _information = information;
            _redisServer = _information.GetSetting("RedisConnection");
            _redisPassword = _information.GetSetting("RedisPassword");
        }

        public IList<RedisStats> GetRedisStats(string controllerName, string controlerversion)
        {
            var stats = new List<RedisStats>();

            try
            {
                var redis = ConnectWithPassword(_redisServer, _redisPassword);

                var db = redis.GetDatabase(0);
                var servers = db.HashGetAll("{Services}:ControllerService");

                for (int i = 0; i < 16; i++)
                {
                    var stat = new RedisStats();
                    var key = "{" + controllerName + "/" + controlerversion + "/" + i + "}";

                    var workers = db.HashGetAll($"{key}:Workers");
                    var queue = db.SortedSetLength($"{key}:Queue");
                    var result = Getworkercounts(workers);

                    stat.RedisHealth = RedisHealth.Stable;
                    stat.Workers = result[0];
                    stat.WorkersIdle = result[1];
                    stat.QueueLength = (int)queue;
                    stat.Priority = i;

                    if ((int)queue > 0 && (int)result[0] == 0)
                        stat.RedisHealth = RedisHealth.Critical;
                    else if ((int)queue > 2000 && stat.RedisHealth != RedisHealth.Critical)
                        stat.RedisHealth = RedisHealth.Warning;

                    stats.Add(stat);
                }

                return stats;
            }
            catch (Exception ex)
            {
                throw;
            }

            return stats;
        }

        public RedisSummary GetRedisSummary(string controllerName, string controlerversion)
        {
            var totalActiveWorkers = 0;
            var totalQueueLength = 0;
            var totalIdleWorkers = 0;

            try
            {
                var redis = ConnectWithPassword(_redisServer, _redisPassword);
                var summary = new RedisSummary();
                var db = redis.GetDatabase(0);
                var servers = db.HashGetAll("{Services}:ControllerService");

                var health = RedisHealth.Stable;

                for (int i = 0; i < 16; i++)
                {
                    var key = "{" + controllerName + "/" + controlerversion + "/" + i + "}";

                    var workers = db.HashGetAll($"{key}:Workers");
                    var queue = db.SortedSetLength($"{key}:Queue");
                    var result = Getworkercounts(workers);

                    totalActiveWorkers += result[0];
                    totalIdleWorkers += result[1];
                    totalQueueLength += (int)queue;

                    if ((int)queue > 0 && (int)result[0] == 0)
                        health = RedisHealth.Critical;
                    else if ((int)queue > 2000 && health != RedisHealth.Critical)
                        health = RedisHealth.Warning;
                }

                summary = new RedisSummary()
                {
                    ControllerName = controllerName,
                    ControllerVersion = controlerversion,
                    QueueLength = totalQueueLength,
                    RedisHealth = health,
                    WorkersIdle = totalIdleWorkers,
                    Workers = totalActiveWorkers
                };

                return summary;
            }
            catch (Exception ex)
            {
                return new RedisSummary()
                {
                    ControllerName = controllerName,
                    ControllerVersion = controlerversion,
                    QueueLength = totalQueueLength,
                    RedisHealth = RedisHealth.Critical,
                    WorkersIdle = totalIdleWorkers,
                    Workers = totalActiveWorkers,
                    Message = ex.Message
                };
            }
        }

        private int[] Getworkercounts(HashEntry[] servers)
        {
            int[] countresult = new int[2];
            countresult[0] = servers.Length;
            var count = 0;
            foreach (var server in servers)
            {
                var workersettings = server.Value.ToString().Split(',');

                var inprogress = workersettings.FirstOrDefault(x => x.Contains("InProgressId"));
                if (inprogress.Length == 17)
                    count++;
            }
            countresult[1] = count;

            return countresult;
        }

        private ConnectionMultiplexer ConnectWithPassword(string server, string password, int retry = 3)
        {
            var redisOptions = new ConfigurationOptions
            {
                EndPoints = { server },
                ConnectTimeout = 20000,
                SyncTimeout = 20000,
                ClientName = AppDomain.CurrentDomain.FriendlyName,
                AllowAdmin = true,
                ReconnectRetryPolicy = new ExponentialRetry(100),
                Password = password
            };

            return ConnectionMultiplexer.Connect(redisOptions);
        }
    }
}