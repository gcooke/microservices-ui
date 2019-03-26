using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Bagl.Cib.MIT.IO;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MIT.Redis;
using Bagl.Cib.MIT.Redis.Caching;
using Gateway.Web.Database;
using Gateway.Web.Models.Request;
using StackExchange.Redis;

namespace Gateway.Web.Services
{
    public class LogsService : ILogsService
    {
        private readonly IGatewayDatabaseService _database;
        private readonly IRedisConnectionProvider _provider;
        private readonly IFileService _fileService;
        private readonly RedisList _gatewaysList;
        private Summary _summary;

        public LogsService(ILoggingService loggingService,
                           IGatewayDatabaseService database,
                           IRedisConnectionProvider provider,
                           IFileService fileService)
        {
            _database = database;
            _provider = provider;
            _fileService = fileService;
            _gatewaysList = CreateRedisList(loggingService);
        }

        private RedisList CreateRedisList(ILoggingService loggingService)
        {
            var config = new RedisContainerConfiguration()
            {
                Namespace = string.Empty,
            };

            return new RedisList(loggingService, _provider, config);
        }

        public Logs GetLogs(string correlationId)
        {
            _summary = _database.GetRequestSummary(correlationId);
            var result = new Logs(correlationId);

            try
            {
                foreach (var item in GetGatewayLogs(correlationId))
                    result.Items.Add(item);

                foreach (var item in GetControllerLogs(correlationId))
                    result.Items.Add(item);
            }
            catch (Exception ex)
            {
                var item = new Log("Exception whilst retrieving logs");
                item.Content = ex.Message;
                result.Items.Add(item);
            }

            return result;
        }

        private IEnumerable<Log> GetGatewayLogs(string correlationId)
        {
            var result = new List<Log>();
            try
            {
                var gateways = _gatewaysList.GetAll("RegisteredGateways");
                foreach (var gateway in gateways)
                {
                    foreach (var log in GetGatewayLogs(gateway, correlationId))
                        result.Add(log);
                }
            }
            catch (Exception ex)
            {
                var item = new Log("Exception whilst retrieving gateway logs");
                item.Content = ex.Message;
                result.Add(item);
            }
            return result;
        }

        private IEnumerable<Log> GetGatewayLogs(string gateway, string correlationId)
        {
            var fileLocation = string.Format(@"\\{0}\data\LogFiles\Gateway\", gateway);
            var fileSearch = string.Format(@"*-Gateway.log");
            var rolledFileSearch = string.Format(@"*-Gateway.log*.log");

            var result = new List<Log>();
            try
            {
                foreach (var file in Directory.GetFiles(fileLocation, fileSearch))
                {
                    // Only open file if it is modified after the start time
                    if (!IsModifiedAfterStartTime(file)) continue;

                    var item = new Log($"gateway logs");
                    item.Location = file;
                    item.Content = GetGatewayLogRelevantContent(file, correlationId);
                    if (!string.IsNullOrEmpty(item.Content))
                        result.Add(item);
                }
                foreach (var file in Directory.GetFiles(fileLocation, rolledFileSearch))
                {
                    // Only open file if it is modified after the start time
                    if (!IsModifiedAfterStartTime(file)) continue;

                    var item = new Log($"gateway logs");
                    item.Location = file;
                    item.Content = GetGatewayLogRelevantContent(file, correlationId);
                    if (!string.IsNullOrEmpty(item.Content))
                        result.Add(item);
                }
            }
            catch (Exception ex)
            {
                var item = new Log("Exception whilst retrieving logs");
                item.Location = Path.Combine(fileLocation, fileSearch);
                item.Content = ex.Message;
                result.Add(item);
            }

            return result;
        }

        private bool IsModifiedAfterStartTime(string file)
        {
            var info = new FileInfo(file);
            // Only include files that were modified or created before the end time
            if (info.LastWriteTimeUtc >= _summary.StartUtc && info.CreationTimeUtc <= _summary.StartUtc)
                return true;
            return false;
        }

        private IEnumerable<Log> GetControllerLogs(string correlationId)
        {
            bool found = false;
            foreach (var item in GetProcessIds(correlationId))
            {
                foreach (var log in GetLogExtract(correlationId, item))
                {
                    yield return log;
                }
                found = true;
            }

            if (!found)
            {
                var result = new Log("controller");
                result.Content = "No controller logs found";
                yield return result;
            }
        }

        public IEnumerable<string> GetRelevantControllerLogFileNames(LogLocation location)
        {
            var fileLocation = string.Format(@"\\{0}\data\LogFiles\{1}\", location.Server, location.Controller);
            var fileSearch = string.Format(@"*_{0}.log.*", location.Pid);
            var rolledDailyFileSearch = string.Format(@"*_{0}.log*.log", location.Pid);

            var result = new List<string>();

            foreach (var file in _fileService.GetFiles(fileLocation, fileSearch))
            {
                result.Add(file);
            }

            foreach (var file in _fileService.GetFiles(fileLocation, rolledDailyFileSearch))
            {
                result.Add(file);
            }

            return result.OrderByDescending(f => f);
        }

        private IEnumerable<Log> GetLogExtract(string correlationId, LogLocation location)
        {
            var result = new List<Log>();
            try
            {
                var isRelevant = false;
                foreach (var file in GetRelevantControllerLogFileNames(location))
                {
                    var item = new Log($"{location.Controller} logs");
                    item.Location = file;
                    item.Content = GetControllerLogRelevantContent(file, correlationId, ref isRelevant);
                    if (!string.IsNullOrEmpty(item.Content))
                        result.Add(item);
                }

                // only take last result
                while (result.Count > 1)
                    result.RemoveAt(0);
            }
            catch (Exception ex)
            {
                var item = new Log("Exception whilst retrieving logs");
                item.Location = string.Empty;
                item.Content = ex.Message;
                result.Add(item);
            }

            return result;
        }

        private string GetControllerLogRelevantContent(string file, string correlationId, ref bool isRelevant)
        {
            var startLine = "Processing Request " + correlationId;
            var endLine = "Completed request: " + correlationId;

            var lines = new List<string>();
            var wasCut = false;

            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (isRelevant)
                        {
                            lines.Add(line);

                            // Check for end
                            if (line != null && line.IndexOf(endLine, StringComparison.CurrentCultureIgnoreCase) >= 0)
                            {
                                isRelevant = false;
                            }
                        }
                        else
                        {
                            // Check for start (Processing Request 4f4a60be-0925-4535-8603-b446c5df7598)
                            if (line != null && line.IndexOf(startLine, StringComparison.CurrentCultureIgnoreCase) >= 0)
                            {
                                isRelevant = true;
                                lines.Add(line);
                            }
                        }

                        if (lines.Count > 200)
                        {
                            wasCut = true;
                            lines.RemoveAt(0);
                        }
                    }
                }
            }

            for (int index = 0; index < lines.Count; index++)
                lines[index] = lines[index].Replace("<", "&lt;").Replace(">", "&gt;");

            if (wasCut)
            {
                lines.Insert(0, "Only last 200 lines shown...");
                lines.Insert(1, "");
            }
            return string.Join("<br/>", lines);
        }

        private string GetGatewayLogRelevantContent(string file, string correlationId)
        {
            var builder = new StringBuilder();
            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        // Check correlation Id
                        if (line != null && line.IndexOf(correlationId, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            builder.Append(line);
                            builder.AppendLine("<br/>");
                        }
                    }
                }
            }
            return builder.ToString();
        }

        private IEnumerable<LogLocation> GetProcessIds(string correlationId)
        {
            var transitions = _database.GetRequestTransitions(correlationId);
            foreach (var transition in transitions.Items)
            {
                if (!transition.Message.Contains("Taken by PID")) continue;

                var message = transition.Message;
                message = message.Substring(13);
                var index = message.IndexOf(" on");
                var pid = message.Substring(0, index);
                message = message.Substring(index + 4);
                var server = message;

                yield return new LogLocation(server, _summary.Controller, pid);
            }
        }

        public class LogLocation
        {
            public LogLocation(string server, string controller, string pid)
            {
                Server = server;
                Controller = controller;
                Pid = pid;
            }

            public string Server { get; set; }
            public string Controller { get; set; }
            public string Pid { get; set; }
        }
    }

    public class RedisList : IRedisDatabaseWrapper
    {
        private volatile IRedisConnectionProvider _connectionProvider;
        private readonly RedisContainerConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly int _database;

        public RedisList(ILoggingService logginService,
            IRedisConnectionProvider connectionProvider,
            RedisContainerConfiguration configuration)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException(nameof(connectionProvider));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _logger = logginService.GetLogger(this);
            _connectionProvider = connectionProvider;
            _configuration = configuration;

            _database = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<IDatabase> GetDatabase()
        {
            IDatabase result = (await _connectionProvider
                    .GetConnection()
                    .ConfigureAwait(false))
                .GetDatabase(_database);

            return result;
        }

        public List<string> GetAll(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Unexpected null or empty string.", nameof(name));

            var database = GetDatabase().Result;
            //var key = $"{_configuration.Namespace ?? string.Empty}{name}";
            var key = $"{name}";

            RedisValue[] value = database.ListRange(key, 0, -1, _configuration.CommandFlags);
            return value.Select(v => v.ToString()).ToList();
        }
    }
}