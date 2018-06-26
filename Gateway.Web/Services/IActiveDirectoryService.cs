using System;
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
    }

    public class ActiveDirectoryService : IActiveDirectoryService
    {
        private readonly string[] _trustedDomains;
        private readonly string _fallbackDomain;
        private ILogger _logger;

        public ActiveDirectoryService(ILoggingService loggingService)
        {
            _trustedDomains = new[] { "D_ABSA", "INTRANET", "CLIENT" };
            _fallbackDomain = "D_ABSA";
            _logger = loggingService.GetLogger(this);
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

        private IEnumerable<UserPrincipal> ResolveUsers(string groupDomain, string groupName)
        {
            var result = new List<UserPrincipal>();
            try
            {
                var fallbackDomain = _trustedDomains.Contains(groupDomain) ? groupDomain : _fallbackDomain;

                // set up domain context
                var groupCtx = new PrincipalContext(ContextType.Domain, fallbackDomain);

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
    }
}