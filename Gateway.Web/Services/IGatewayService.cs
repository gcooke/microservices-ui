using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models.AddIn;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Models.Group;
using Gateway.Web.Models.Permission;
using Gateway.Web.Models.Request;
using Gateway.Web.Models.Security;
using Gateway.Web.Models.User;

namespace Gateway.Web.Services
{
    public interface IGatewayService
    {
        ServersModel GetServers();

        WorkersModel GetWorkers();

        WorkersModel GetWorkers(string controller);

        IEnumerable<QueueModel> GetCurrentQueues(string controller);

        IEnumerable<QueueModel> GetCurrentQueues();

        XElement[] GetReport(string url);

        string[] GetSites();

        VersionsModel GetControllerVersions(string name);

        void ExpireWorkItem(string id);

        string[] UpdateControllerVersionStatuses(List<VersionUpdate> versionStatusUpdates);

        RequestPayload GetRequestTree(Guid correlationId);

        ConfigurationModel GetControllerConfiguration(string name);

        RestResponse UpdateControllerConfiguration(ConfigurationModel model);

        Models.Security.GroupsModel GetGroups();

        Models.Security.ReportsModel GetSecurityReport(string name);

        Models.Security.ReportsModel GetSecurityReport(string name, string parameter);

        Models.Group.GroupModel GetGroup(long id);

        string[] Create(Models.Group.GroupModel model);

        string[] DeleteGroup(long id);

        Models.Security.UsersModel GetUsers();

        Models.User.UserModel GetUser(string login);

        Models.Security.AddInsModel GetAddIns();

        Models.AddIn.AddInModel GetAddIn(long id);

        string[] Create(Models.AddIn.AddInModel model);

        string[] DeleteAddIn(long id);

        Models.Security.PermissionsModel GetPermissions();

        Models.Permission.PermissionModel GetPermission(long id);

        string[] DeletePermission(long id);

        string[] Create(Models.Permission.PermissionModel model);

        string[] InsertGroupPermission(long groupId, long permissionId);

        ADGroupsModel GetGroupADGroups(long groupId);

        string[] DeleteGroupADGroup(long id, long groupId);

        string[] Create(GroupActiveDirectory model);

        Models.Group.PermissionsModel GetGroupPermisions(long groupId);

        Models.Group.PortfoliosModel GetGroupPortfolios(long groupId);

        Models.Group.SitesModel GetGroupSites(long groupId);

        string[] DeleteGroupSite(long id, long groupId);

        string[] InsertGroupSite(long groupId, long siteId);

        Models.Group.AddInsModel GetGroupAddIns(long groupId);

        string[] InsertGroupAddInVersion(long groupId, AddInVersionModel addInVersion);

        string[] DeleteGroupAddInVersion(long id, long groupId);
    }
}