using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Web;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Models.Group;
using Gateway.Web.Models.User;

namespace Gateway.Web.Services
{
    public interface IActiveDirectoryService
    {
        UsersModel GetUsers(ADGroupsModel adgroups);
        UserPrincipal FindUser(string domain, string name);
    }

    public class ActiveDirectoryService : IActiveDirectoryService
    {
        public const string DefaultDomain = "D_ABSA";
        private readonly ConcurrentDictionary<string, PrincipalContext> _contexts;
        private readonly string[] _trustedDomains;
        private ILogger _logger;

        public ActiveDirectoryService(ILoggingService loggingService)
        {
            _trustedDomains = new[] { "D_ABSA", "INTRANET", "CLIENT" };
            _logger = loggingService.GetLogger(this);
            _contexts = new ConcurrentDictionary<string, PrincipalContext>(StringComparer.CurrentCultureIgnoreCase);
        }

        public UsersModel GetUsers(ADGroupsModel adgroups)
        {
            var result = new UsersModel(0);
            foreach (var group in adgroups.Items)
            {
                foreach (var user in ResolveUsers(group.Domain, group.Name))
                {
                    var name = user.Sid.Translate(typeof(NTAccount)).ToString();
                    var names = name.Split('\\');
                    result.Items.Add(new UserModel()
                    {
                        FullName = user.DisplayName,
                        Login= names[1],
                        Domain = names[0]
                    });
                }
            }
            return result;
        }

        public UserPrincipal FindUser(string domain, string name)
        {
            var context = GetDomainContext(domain);
            return GetUserPrincipal(context, name);
        }

        private UserPrincipal GetUserPrincipal(PrincipalContext groupCtx, string userName, string userSid = null)
        {
            if (string.IsNullOrEmpty(userSid))
            {
                return UserPrincipal.FindByIdentity(groupCtx, IdentityType.SamAccountName, userName);
            }
            return UserPrincipal.FindByIdentity(groupCtx, IdentityType.Sid, userSid);
        }

        private IEnumerable<UserPrincipal> ResolveUsers(string groupDomain, string groupName)
        {
            var result = new List<UserPrincipal>();
            try
            {
                var fallbackDomain = _trustedDomains.Contains(groupDomain) ? groupDomain : DefaultDomain;

                // set up domain context
                var groupCtx = GetDomainContext(fallbackDomain);

                // find the group in question
                GroupPrincipal group = GroupPrincipal.FindByIdentity(groupCtx, groupName);

                // if found....
                if (group != null)
                {
                    // iterate over members
                    foreach (Principal p in group.GetMembers())
                    {
                        var theUser = p as UserPrincipal;
                        if (theUser == null) continue;

                        result.Add(theUser);

                        // Limit results
                        if (result.Count > 500) break;
                    }
                }
                else
                {
                    _logger.InfoFormat("Could not find group {0}/{1}", fallbackDomain, groupName);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.InfoFormat("Failed to resolve user {0}/{1}: {2}", groupDomain, groupName, ex.Message);
            }

            return result;
        }

        private PrincipalContext GetDomainContext(string domain)
        {
            return _contexts.GetOrAdd(domain, s => new PrincipalContext(ContextType.Domain, s));
        }
    }
}