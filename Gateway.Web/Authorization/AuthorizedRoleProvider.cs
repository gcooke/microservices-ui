using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using Absa.Cib.Authorization.Extensions;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Provider;
using CommonServiceLocator;
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
            var container = (IUnityContainer)HttpContext.Current.Items[MvcApplication.ContainerKey];
            var authen = container.Resolve<IAuthenticationProvider>();
            var currenttoken = authen.GetToken();

            if (string.IsNullOrEmpty(currenttoken))
            {
                var cookie = HttpContext.Current.Request.Cookies[GatewayAuthenticationFilter.AuthCockieName];

                if (cookie == null)
                {
                    _logger.Info($"No Token Found IsUserInRole for {username}");
                    return false;
                }

                authen.SetToken(cookie.Value);
            }
               
            var auhtorize = container.Resolve<IAuthorizationProvider>();
            var roles = auhtorize.GetClaims<string>(currenttoken, ClaimTypes.Role);

            if (roles != null)
            {
                return roles.Where(e => e.Equals(roleName,StringComparison.InvariantCultureIgnoreCase)).Any();
            }
            else
            {
                _logger.Info($"Checking Right for user : {username} using Token {currenttoken}");
                _logger.Info($"Roles in Token : {String.Join(",", roles)}");
                _logger.Error($"User {username} is not in role {roleName}");
            }
            return false;
        }

        public override string[] GetRolesForUser(string username)
        {
            var container = (IUnityContainer)HttpContext.Current.Items[MvcApplication.ContainerKey];
            var authen = container.Resolve<IAuthenticationProvider>();
            var currenttoken = authen.GetToken();

            if (string.IsNullOrEmpty(currenttoken))
            {   
                var cookie = HttpContext.Current.Request.Cookies[GatewayAuthenticationFilter.AuthCockieName];

                if (cookie == null)
                {
                    _logger.Info($"No Token Found GetRolesForUser for {username}");
                    return new string[] { };
                }

                authen.SetToken(cookie.Value);
            }

            var auhtorize = container.Resolve<IAuthorizationProvider>();
            var roles = auhtorize.GetClaims<string>(currenttoken, ClaimTypes.Role);
            
            if (roles != null)
            {
                return roles.ToArray();
            }
            else
            {
                _logger.Info($"Checking Right for user : {username} using Token {currenttoken}");
                _logger.Info($"Roles in Token : {String.Join(",", roles)}");
                _logger.Error($"User {username} does not have roles defined for Dashboard access.");
            }
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
    }
};