using System;
using System.Collections.Concurrent;
using Gateway.Web.Database;

namespace Gateway.Web.Services
{
    public class BatchNameService : IBatchNameService
    {
        private readonly ConcurrentDictionary<string, string> _batchNames;
        private readonly IGatewayDatabaseService _databaseService;

        public BatchNameService(IGatewayDatabaseService databaseService)
        {
            _databaseService = databaseService;
            _batchNames = new ConcurrentDictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        }

        public string GetName(string resource)
        {
            return _batchNames.GetOrAdd(resource, GetNameFromDatabase);
        }

        private string GetNameFromDatabase(string resource)
        {
            var original = resource;
            resource = resource.Substring(10);
            resource = resource.Substring(0, resource.IndexOf("/", StringComparison.Ordinal));
            var date = resource.Substring(resource.LastIndexOf("/", StringComparison.Ordinal) + 1);
            if (!int.TryParse(resource, out var id)) return original;

            var result = _databaseService.GetBatchName(id);
            return result == null ? original : $"{result} ({date})";
        }
    }
}