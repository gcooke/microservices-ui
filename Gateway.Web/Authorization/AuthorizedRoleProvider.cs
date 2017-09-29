﻿using System.Linq;
using System.Web.Security;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Microsoft.Practices.ServiceLocation;

namespace Gateway.Web.Authorization
{
    public class AuthorizedRoleProvider : RoleProvider
    {
        private readonly IRoleService _roleService;
        private readonly ILogger _logger;

        public AuthorizedRoleProvider()
        {
            _roleService = ServiceLocator.Current.GetInstance<IRoleService>();
            var loggingService = ServiceLocator.Current.GetInstance<ILoggingService>();;
            _logger = loggingService.GetLogger(this);
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            var userDetail = _roleService.GetUserDetail(username);
            if (userDetail != null)
            {
                return userDetail.HasRole(roleName);
            }
            return false;
        }

        public override string[] GetRolesForUser(string username)
        {
            var userDetail = _roleService.GetUserDetail(username);
            if (userDetail != null)
            {
                return userDetail.Roles
                    .Where(r => r.SystemName == "Redstone Dashboard")
                    .Select(r => r.Name).ToArray();
            }
            else
            {
                _logger.ErrorFormat("User {0} does not have roles defined for Dashboard access.", username);
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