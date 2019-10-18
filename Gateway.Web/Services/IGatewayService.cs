using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bagl.Cib.MSF.ClientAPI.Model;
using Gateway.Web.Controllers;
using Gateway.Web.Models.AddIn;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Models.Group;
using Gateway.Web.Models.MarketData;
using Gateway.Web.Models.Security;
using Gateway.Web.Models.Shared;
using Gateway.Web.Models.User;
using VersionsModel = Gateway.Web.Models.Controller.VersionsModel;

namespace Gateway.Web.Services
{
    public interface IGatewayService
    {
        Task<ServersModel> GetServers();

        Task<List<ServiceInfoModel>> GetWorkersAsync();

        Task<List<ServiceInfoModel>> GetWorkersAsync(string controller);

        Task<IEnumerable<QueueModel>> GetCurrentQueues(string controller);

        Task<IEnumerable<QueueModel>> GetCurrentQueues();

        Task<XElement[]> GetReport(string url);

        Task<List<SiteModel>> GetSites();

        Task<VersionsModel> GetControllerVersions(string name);

        void ExpireWorkItem(string id);

        Task<string[]> UpdateControllerVersionStatuses(List<VersionUpdate> versionStatusUpdates);

        Task<ConfigurationModel> GetControllerConfiguration(string name);

        Task<GatewayResponse<string>> UpdateControllerConfiguration(ConfigurationModel model);

        Task<Models.Security.GroupsModel> GetGroups();

        Task<List<PortfolioModel>> GetPortfolios();

        Task<Models.Security.ReportsModel> GetSecurityReport(string name);

        Task<Models.Security.ReportsModel> GetSecurityReport(string name, string parameterName, string parameter);

        Task<Models.Group.GroupModel> GetGroup(long id);

        Task<string[]> Create(Models.Group.GroupModel model);

        Task<string[]> DeleteGroup(long id);

        Task<Models.Security.UsersModel> GetUsers();

        Task<Models.User.UserModel> GetUser(string id);

        Task<Models.Security.ApplicationsModel> GetApplications();

        Task<AddInModel> GetAddIn(long id);

        Task<IEnumerable<ApplicationVersionModel>> GetApplicationVersions();

        Task<IEnumerable<AddInVersionModel>> GetAddInVersions();

        Task<string[]> Create(ApplicationModel model);

        Task<string[]> Create(AddInModel model);

        Task<string[]> DeleteAddIn(long id);

        Task<Models.Security.PermissionsModel> GetPermissions();

        Task<Models.Permission.PermissionModel> GetPermission(long id);

        Task<string[]> DeleteGroupPermission(long id, long groupId);

        Task<string[]> DeleteGroupPortfolio(long id, long groupId);

        Task<string[]> Create(Models.Permission.PermissionModel model);

        Task<string[]> InsertGroupPermission(long groupId, long permissionId);

        Task<ADGroupsModel> GetGroupADGroups(long groupId);

        Task<string[]> DeleteGroupADGroup(long id, long groupId);

        Task<string[]> Create(GroupActiveDirectory model);

        Task<Models.Group.PermissionsModel> GetGroupPermisions(long groupId);

        Task<Models.Group.PortfoliosModel> GetGroupPortfolios(long groupId);

        Task<Models.Group.SitesModel> GetGroupSites(long groupId);

        Task<Models.Group.UsersModel> GetGroupUsers(long groupId);

        Task<string[]> DeleteGroupSite(long id, long groupId);

        Task<string[]> InsertGroupSite(long groupId, long siteId);

        Task RetryWorkItemAsync(string correlationId);

        Task<Models.Group.AddInsModel> GetGroupAddIns(long groupId);

        Task<string[]> InsertGroupAddInVersion(long groupId, AddInVersionModel addInVersion);

        Task<string[]> InsertGroupApplicationVersion(long groupId, ApplicationVersionModel addInVersion);

        Task<string[]> DeleteGroupAddInVersion(long id, long groupId);

        Task<string[]> DeleteGroupApplicationVersion(long id, long groupId);

        Task<string[]> UpdateAssignedApplicationVersions(string from, string to);

        Task<string[]> UpdateAssignedAddInVersions(string from, string to);

        Task<string[]> UpdateGroupBusinessFunction(string groupId, string businessFunctionId);

        #region User Security Configuration

        Task<string[]> RemoveUser(long id);

        Task<string[]> Create(UserModel model);

        #endregion User Security Configuration

        #region User Group Security Configuration

        Task<Models.User.UserModel> GetUserGroups(long id);

        Task<string[]> InsertUserGroup(long userId, long groupId);

        Task<string[]> RemoveUserGroup(long userId, long groupId);

        #endregion User Group Security Configuration

        #region User Portfolio Security Configuration

        Task<Models.User.PortfoliosModel> GetUserPortfolios(long userId);

        Task<string[]> InsertUserPortfolio(long userId, long portfolioId);

        Task<string[]> RemoveUserPortfolio(long userId, long portfolioId);

        #endregion User Portfolio Security Configuration

        #region User Sites Security Configuration

        Task<Models.User.SitesModel> GetUserSites(long userId);

        Task<string[]> InsertUserSite(long userId, long siteId);

        Task<string[]> RemoveUserSite(long userId, long siteId);

        #endregion User Sites Security Configuration

        #region User Add In Version Security Configuration

        Task<Models.User.AddInsModel> GetUserAddInVersions(long userId);

        Task<string[]> DeleteUserAddInVersions(long userId, long addInVersionId);

        Task<string[]> DeleteUserApplicationVersions(long userId, long applicationVersionId);

        Task<string[]> InsertUserAddInVersions(long groupId, AddInVersionModel addInVersion);

        Task<string[]> InsertUserApplicationVersions(long groupId, ApplicationVersionModel version);

        #endregion User Add In Version Security Configuration

        #region Business Functions

        Task<IEnumerable<BusinessFunction>> GetBusinessFunctions();

        Task<string[]> Create(BusinessFunction model);

        Task<string[]> DeleteBusinessFunction(int id);

        #endregion Business Functions

        #region GroupTypes

        Task<IEnumerable<GroupType>> GetGroupTypes();

        Task<string[]> Create(GroupType model);

        Task<string[]> DeleteGroupType(int id);

        #endregion GroupTypes

        Task<bool> GenerateDocumentation(string id, string version);

        Task CancelWorkItemAsync(string correlationId);

        Task<List<MonikerCheckResult>> GetMonikers(string server, string query);

        Task<string> GetAsync(string gateway, string query);

        Task KillWorkersAsync();

        Task KillWorkersAsync(string controller);

        Task KillWorkerAsync(string queuename, string id);

        Task RequestWorkersAsync(RequestedWorkers requestedWorkers);

        Task ShutdownWorkersAsync(string controller);

        Task ShutdownWorkerAsync(string queuename, string id);

        void NotifyResourceUpdate();

        Task<GatewayResponse<TResponse>> Send<TResponse>(GatewayRequest request);
    }
}
