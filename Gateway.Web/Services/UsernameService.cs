using System;
using System.Collections.Concurrent;
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

        public string GetFullNameFast(string name)
        {
            if (_lookup.TryGetValue(name, out var result))
                return result;

            result = GetUserNameFromActiveDirectory(name, true);
            if (result != name)
                _lookup.TryAdd(name, result);
            return result;
        }

        public string GetFullName(string name)
        {
            return _lookup.GetOrAdd(name, (n) => GetUserNameFromActiveDirectory(n, false));
        }

        private string GetUserNameFromActiveDirectory(string username, bool fast)
        {
            if (string.IsNullOrEmpty(username)) return username;
            if (!username.Contains("\\")) return username;

            var parts = username.Split('\\');
            var domain = parts.First();
            var account = parts.Skip(1).Take(1).FirstOrDefault();

            // Do not lookup user if fast lookup is requested and domain is not default domain
            if (fast && !string.Equals(domain, ActiveDirectoryService.DefaultDomain)) return username;

            var result = _activeDirectoryService.FindUser(domain, account);
            if (result == null) return username;
            return result.DisplayName ?? result.Name;
        }
    }
}