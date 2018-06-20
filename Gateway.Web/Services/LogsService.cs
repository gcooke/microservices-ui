using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gateway.Web.Database;
using Gateway.Web.Models.Request;

namespace Gateway.Web.Services
{
    public class LogsService : ILogsService
    {
        private readonly IGatewayDatabaseService _database;

        public LogsService(IGatewayDatabaseService database)
        {
            _database = database;
        }

        public Logs GetLogs(string correlationId)
        {
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
            var result = new Log("gateway logs");
            result.Content = "Not implemented";
            yield return result;
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

        private IEnumerable<Log> GetLogExtract(string correlationId, LogLocation location)
        {
            var fileLocation = string.Format(@"\\{0}\data\LogFiles\{1}\", location.Server, location.Controller);
            var fileSearch = string.Format(@"*_{0}.log", location.Pid);

            var result = new List<Log>();
            try
            {
                foreach (var file in Directory.GetFiles(fileLocation, fileSearch))
                {
                    var item = new Log($"{location.Controller} logs");
                    item.Location = file;
                    item.Content = GetControllerLogRelevantContent(file, correlationId);
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

        private string GetControllerLogRelevantContent(string file, string correlationId)
        {
            var startLine = "Processing Request " + correlationId;
            var endLine = "Completed request: " + correlationId;

            var builder = new StringBuilder();
            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var isRelevant = false;
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (isRelevant)
                        {
                            builder.Append(line);
                            builder.AppendLine("<br/>");

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
                                builder.Append(line);
                                builder.AppendLine("<br/>");
                            }
                        }
                    }
                }
            }
            return builder.ToString();
        }

        //private string GetGatewayLogRelevantContent(string file, string correlationId)
        //{
        //    var builder = new StringBuilder();
        //    using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        //    {
        //        using (var reader = new StreamReader(stream))
        //        {
        //            while (!reader.EndOfStream)
        //            {
        //                var line = reader.ReadLine();

        //                // Check correlation Id
        //                if (line != null && line.IndexOf(correlationId, StringComparison.CurrentCultureIgnoreCase) >= 0)
        //                {
        //                    builder.Append(line);
        //                    builder.AppendLine("<br/>");
        //                }
        //            }
        //        }
        //    }
        //    return builder.ToString();
        //}

        private IEnumerable<LogLocation> GetProcessIds(string correlationId)
        {
            var summary = _database.GetRequestSummary(correlationId);
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

                yield return new LogLocation(server, summary.Controller, pid);
            }
        }

        private class LogLocation
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
}