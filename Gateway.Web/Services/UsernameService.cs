using System;
using System.Collections.Concurrent;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace Gateway.Web.Services
{
    public class UsernameService : IUsernameService
    {
        private readonly IActiveDirectoryService _activeDirectoryService;
        private readonly ConcurrentDictionary<string, string> _lookup;

        public UsernameService(IActiveDirectoryService activeDirectoryService)
        {
            _activeDirectoryService = activeDirectoryService;
            _lookup = new ConcurrentDictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        }

        public string GetFullName(string name)
        {
            return _lookup.GetOrAdd(name, GetUserNameFromActiveDirectory);
        }

        private string GetUserNameFromActiveDirectory(string username)
        {
            if (string.IsNullOrEmpty(username)) return username;
            if (!username.Contains("\\")) return username;

            var parts = username.Split('\\');
            var domain = parts.First();
            var account = parts.Skip(1).Take(1).FirstOrDefault();

            var result = _activeDirectoryService.FindUser(domain, account);
            if (result == null) return username;
            return result.DisplayName ?? result.Name;
        }
    }
}