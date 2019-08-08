using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Absa.Cib.Authorization.Extensions;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Provider;
using CommonServiceLocator;
using Gateway.Web.Models;
using Unity;

namespace Gateway.Web.Authorization
{
    public class AuthorizedRoleProvider : RoleProvider
    {
        private readonly ILogger _logger;

        public AuthorizedRoleProvider()
        {
            var loggingService = ServiceLocator.Current.GetInstance<ILoggingService>();
            _logger = loggingService.GetLogger(this);
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            var info = GetUserInformation(_logger);

            if (info.Roles.Count > 0)
            {
                return info.Roles.Any(e => e.Equals(roleName, StringComparison.InvariantCultureIgnoreCase));
            }

            _logger.Info($"Checking Right for user : {username} using Token {info.Token}");
            _logger.Info($"Roles in Token : {string.Join(",", info.Roles)}");
            _logger.Error($"User {username} is not in role {roleName}");
            return false;
        }

        public override string[] GetRolesForUser(string username)
        {
            var info = GetUserInformation(_logger);

            if (info.Roles.Count > 0)
            {
                return info.Roles.ToArray();
            }

            _logger.Info($"Checking Right for user : {username} using Token {info.Token}");
            _logger.Info($"Roles in Token : {string.Join(",", info.Roles)}");
            _logger.Error($"User {username} does not have roles defined for Dashboard access.");
            return new string[] { };
        }

        public override void CreateRole(string roleName)
        {
            throw new System.NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new System.NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new System.NotImplementedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new System.NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new System.NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new System.NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new System.NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new System.NotImplementedException();
        }

        public override string ApplicationName { get; set; }

        public static AuthorizationModel GetUserInformation(ILogger logger)
        {
            var container = HttpContainerProvider.Container;
            var authen = container.Resolve<IAuthenticationProvider>();
            var currenttoken = authen.GetToken();
            var result = new AuthorizationModel();

            if (string.IsNullOrEmpty(currenttoken))
            {
                var cookie = HttpContext.Current.Request.Cookies[GatewayAuthenticationFilter.AuthCockieName];

                if (cookie == null)
                {
                    logger.Info($"No Token Found");
                    return result;
                }

                authen.SetToken(cookie.Value);
                currenttoken = authen.GetToken();
            }

            var authorize = container.Resolve<IAuthorizationProvider>();
           // result.Username = authorize.GetClaims<string>(currenttoken, ClaimTypes.Username)?.FirstOrDefault();
           // result.Roles = authorize.GetClaims<string>(currenttoken, ClaimTypes.Role) ?? new List<string>();
            return result;
        }
    }
};