using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Redis;
using Gateway.Web.Models.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace Gateway.Web.Services
{
    public class RedisService : IRedisService
    {
        public readonly string _redisPassword;
        public readonly string _redisServer;
        private readonly ISystemInformation _information;
        private readonly IRedisConnectionProvider _redisConnectionProvider;

        public RedisService(ISystemInformation information, IRedisConnectionProvider redisConnectionProvider)
        {
            _information = information;
            _redisConnectionProvider = redisConnectionProvider;
            _redisServer = _information.GetSetting("RedisConnection");
            _redisPassword = _redisConnectionProvider?.Options?.RedisOptions.Password;
        }

        public IList<RedisStats> GetRedisStats(string controllerName, string controlerversion, int maxPriority)
        {
            var stats = new List<RedisStats>();

            try
            {
                var redis = ConnectWithPassword(_redisServer, _redisPassword);

                var db = redis.GetDatabase(0);
                var servers = db.HashGetAll("{Services}:ControllerService");

                for (int i = 0; i < maxPriority + 1; i++)
                {
                    var stat = new RedisStats();
                    var key = "{" + controllerName + "/" + controlerversion + "/" + i + "}";

                    var workers = db.HashGetAll($"{key}:Workers");
                    var queue = db.SortedSetLength($"{key}:Queue");
                    var busyWorkers = db.HashGetAll($"{key}:NackRequests");

                    stat.RedisHealth = RedisHealth.Stable;
                    stat.Workers = workers.Length;
                    stat.WorkersIdle = workers.Length - busyWorkers.Length;
                    stat.QueueLength = (int)queue;
                    stat.Priority = i;

                    if ((int)queue > 0 && workers.Length == 0)
                        stat.RedisHealth = RedisHealth.Critical;
                    else if ((int)queue > 1000 && stat.RedisHealth != RedisHealth.Critical)
                        stat.RedisHealth = RedisHealth.Warning;

                    stats.Add(stat);
                }

                return stats;
            }
            catch
            {
                throw;
            }
        }

        public RedisSummary GetRedisSummary(string controllerName, string controlerversion, int maxPriority)
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

                for (int i = 0; i < maxPriority + 1; i++)
                {
                    var key = "{" + controllerName + "/" + controlerversion + "/" + i + "}";

                    var workers = db.HashGetAll($"{key}:Workers");
                    var queue = db.SortedSetLength($"{key}:Queue");
                    var busyWorkers = db.HashGetAll($"{key}:NackRequests");

                    totalActiveWorkers += workers.Length;
                    totalIdleWorkers += workers.Length - busyWorkers.Length;
                    totalQueueLength += (int)queue;

                    if ((int)queue > 0 && workers.Length == 0)
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
                    Workers = totalActiveWorkers,
                    MaxPriority = maxPriority
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

        public IList<string> GetWorkerids(string controllerName, string controlerversion, int priority)
        {
            var correlationIds = new List<string>();
            var redis = ConnectWithPassword(_redisServer, _redisPassword);
            var db = redis.GetDatabase(0);
            var key = "{" + controllerName + "/" + controlerversion + "/" + priority + "}";
            var busyWorkers = db.HashGetAll($"{key}:NackRequests");
            foreach (var item in busyWorkers)
                correlationIds.Add(item.Name);

            return correlationIds;
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