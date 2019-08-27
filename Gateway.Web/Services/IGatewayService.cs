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
        ServersModel GetServers();

        Task<List<ServiceInfoModel>> GetWorkersAsync();

        Task<List<ServiceInfoModel>> GetWorkersAsync(string controller);

        IEnumerable<QueueModel> GetCurrentQueues(string controller);

        IEnumerable<QueueModel> GetCurrentQueues();

        XElement[] GetReport(string url);

        List<SiteModel> GetSites();

        VersionsModel GetControllerVersions(string name);

        void ExpireWorkItem(string id);

        string[] UpdateControllerVersionStatuses(List<VersionUpdate> versionStatusUpdates);

        ConfigurationModel GetControllerConfiguration(string name);

        GatewayResponse<string> UpdateControllerConfiguration(ConfigurationModel model);

        Models.Security.GroupsModel GetGroups();

        List<PortfolioModel> GetPortfolios();

        Models.Security.ReportsModel GetSecurityReport(string name);

        Models.Security.ReportsModel GetSecurityReport(string name, string parameterName, string parameter);

        Models.Group.GroupModel GetGroup(long id);

        string[] Create(Models.Group.GroupModel model);

        string[] DeleteGroup(long id);

        Models.Security.UsersModel GetUsers();

        Models.User.UserModel GetUser(string id);

        Models.Security.ApplicationsModel GetApplications();

        AddInModel GetAddIn(long id);

        IEnumerable<ApplicationVersionModel> GetApplicationVersions();

        IEnumerable<AddInVersionModel> GetAddInVersions();

        string[] Create(ApplicationModel model);

        string[] Create(AddInModel model);

        string[] DeleteAddIn(long id);

        Models.Security.PermissionsModel GetPermissions();

        Models.Permission.PermissionModel GetPermission(long id);

        string[] DeleteGroupPermission(long id, long groupId);

        string[] DeleteGroupPortfolio(long id, long groupId);

        string[] Create(Models.Permission.PermissionModel model);

        string[] InsertGroupPermission(long groupId, long permissionId);

        ADGroupsModel GetGroupADGroups(long groupId);

        string[] DeleteGroupADGroup(long id, long groupId);

        string[] Create(GroupActiveDirectory model);

        Models.Group.PermissionsModel GetGroupPermisions(long groupId);

        Models.Group.PortfoliosModel GetGroupPortfolios(long groupId);

        Models.Group.SitesModel GetGroupSites(long groupId);

        Models.Group.UsersModel GetGroupUsers(long groupId);

        string[] DeleteGroupSite(long id, long groupId);

        string[] InsertGroupSite(long groupId, long siteId);

        Task RetryWorkItemAsync(string correlationId);

        Models.Group.AddInsModel GetGroupAddIns(long groupId);

        string[] InsertGroupAddInVersion(long groupId, AddInVersionModel addInVersion);

        string[] InsertGroupApplicationVersion(long groupId, ApplicationVersionModel addInVersion);

        string[] DeleteGroupAddInVersion(long id, long groupId);

        string[] DeleteGroupApplicationVersion(long id, long groupId);

        string[] UpdateAssignedApplicationVersions(string from, string to);

        string[] UpdateAssignedAddInVersions(string from, string to);

        string[] UpdateGroupBusinessFunction(string groupId, string businessFunctionId);

        #region User Security Configuration

        string[] RemoveUser(long id);

        string[] Create(UserModel model);

        #endregion User Security Configuration

        #region User Group Security Configuration

        Models.User.UserModel GetUserGroups(long id);

        string[] InsertUserGroup(long userId, long groupId);

        string[] RemoveUserGroup(long userId, long groupId);

        #endregion User Group Security Configuration

        #region User Portfolio Security Configuration

        Models.User.PortfoliosModel GetUserPortfolios(long userId);

        string[] InsertUserPortfolio(long userId, long portfolioId);

        string[] RemoveUserPortfolio(long userId, long portfolioId);

        #endregion User Portfolio Security Configuration

        #region User Sites Security Configuration

        Models.User.SitesModel GetUserSites(long userId);

        string[] InsertUserSite(long userId, long siteId);

        string[] RemoveUserSite(long userId, long siteId);

        #endregion User Sites Security Configuration

        #region User Add In Version Security Configuration

        Models.User.AddInsModel GetUserAddInVersions(long userId);

        string[] DeleteUserAddInVersions(long userId, long addInVersionId);

        string[] DeleteUserApplicationVersions(long userId, long applicationVersionId);

        string[] InsertUserAddInVersions(long groupId, AddInVersionModel addInVersion);

        string[] InsertUserApplicationVersions(long groupId, ApplicationVersionModel version);

        #endregion User Add In Version Security Configuration

        #region Business Functions

        IEnumerable<BusinessFunction> GetBusinessFunctions();

        string[] Create(BusinessFunction model);

        string[] DeleteBusinessFunction(int id);

        #endregion Business Functions

        #region GroupTypes

        IEnumerable<GroupType> GetGroupTypes();

        string[] Create(GroupType model);

        string[] DeleteGroupType(int id);

        #endregion GroupTypes

        bool GenerateDocumentation(string id, string version);

        Task CancelWorkItemAsync(string correlationId);

        List<MonikerCheckResult> GetMonikers(string server, string query);

        Task<string> GetAsync(string gateway, string query);

        Task KillWorkersAsync();

        Task KillWorkersAsync(string controller);

        Task KillWorkerAsync(string queuename, string id);

        Task RequestWorkersAsync(RequestedWorkers requestedWorkers);

        Task ShutdownWorkersAsync(string controller);

        Task ShutdownWorkerAsync(string queuename, string id);
    }
}
