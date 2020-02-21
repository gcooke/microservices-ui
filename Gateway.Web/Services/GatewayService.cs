using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Bagl.Cib.MSF.ClientAPI.Model;
using Gateway.Web.Controllers;
using Gateway.Web.Models.AddIn;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Models.Group;
using Gateway.Web.Models.MarketData;
using Gateway.Web.Models.Permission;
using Gateway.Web.Models.Security;
using Gateway.Web.Models.Shared;
using Gateway.Web.Models.User;
using Gateway.Web.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Linq;
using WebGrease.Css.Extensions;
using ApplicationsModel = Gateway.Web.Models.Security.ApplicationsModel;
using GroupsModel = Gateway.Web.Models.Security.GroupsModel;
using PermissionsModel = Gateway.Web.Models.Security.PermissionsModel;
using PortfoliosModel = Gateway.Web.Models.Group.PortfoliosModel;
using SitesModel = Gateway.Web.Models.Group.SitesModel;
using String = System.String;
using UsersModel = Gateway.Web.Models.Security.UsersModel;
using Version = Gateway.Web.Models.Controller.Version;
using VersionsModel = Gateway.Web.Models.Controller.VersionsModel;

namespace Gateway.Web.Services
{
    public class GatewayService : IGatewayService
    {
        private readonly TimeSpan _defaultRequestTimeout;
        private readonly IGateway _gateway;
        private readonly string[] _gateways;
        private readonly ILogger _logger;
        private readonly int _port = 7010;
        private readonly HttpMessageHandler _handler;
        private readonly Lazy<HttpClient> _defaultClient;

        public GatewayService(
            ISystemInformation information,
            IGateway gateway,
            ILoggingService loggingService
            )
        {
            _defaultRequestTimeout = TimeSpan.FromSeconds(300);
            _gateway = gateway;
            _logger = loggingService.GetLogger(this);
            var gateways = information.GetSetting("KnownGateways", GetDefaultKnownGateways(information.EnvironmentName));
            _gateways = gateways.Split(';');

            //_gateway.SetGatewayUrlForService("Security", "http://localhost:7000/");
            //_gateway.SetGatewayUrlForService("Catalogue", "http://localhost:7000/");

            _defaultClient = new Lazy<HttpClient>(() =>
            {
                var handler = new HttpClientHandler
                {
                    UseDefaultCredentials = true,
                    AllowAutoRedirect = true
                };
                var client = new HttpClient(handler);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = _defaultRequestTimeout;
                return client;
            });
        }

        public async Task<ServersModel> GetServers()
        {
            var gatewayInfo = await GetGatewayInfo();
            return new Models.Controllers.ServersModel(gatewayInfo);
        }

        public async Task<List<ServiceInfoModel>> GetWorkersAsync()
        {
            var gateway = _gateways.FirstOrDefault();
            var model = await GetAsync(gateway, $"Worker/Controllers");
            return JsonConvert.DeserializeObject<List<ServiceInfoModel>>(model);
        }

        public async Task<List<ServiceInfoModel>> GetWorkersAsync(string controller)
        {
            var gateway = _gateways.FirstOrDefault();
            var model = await GetAsync(gateway, $"Worker/Controller/{controller}");
            return JsonConvert.DeserializeObject<List<ServiceInfoModel>>(model);
        }

        public async Task<IEnumerable<QueueModel>> GetCurrentQueues(string controller)
        {
            var docs = await FetchAll("health/queues/{0}", controller);
            return GetCurrentQueues(docs);
        }

        public async Task<IEnumerable<QueueModel>> GetCurrentQueues()
        {
            var docs = await FetchAll("health/queues");
            return GetCurrentQueues(docs);
        }

        public async Task<XElement[]> GetReport(string report)
        {
            //var doc = Fetch("api/riskdata/latest/{0}", report);
            var response = await _gateway.GetSync("riskdata", report, string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            return response.Body.Descendants("xVAReturnResult").ToArray();
        }

        public async Task<List<SiteModel>> GetSites()
        {
            var response = await _gateway.GetSync("Security", "sites", string.Empty);
            var result = new List<SiteModel>();

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            element.Descendants("Site").ForEach(item => { result.Add(item.Deserialize<SiteModel>()); });

            return result;
        }

        public void ExpireWorkItem(string id)
        {
            var result = Delete($"health/queueitem/{id}");
            result.Wait(2000);
        }

        public async Task<VersionsModel> GetControllerVersions(string name)
        {
            var query = string.Format("controllers/{0}", name);
            var response = await _gateway.GetSync("Catalogue", query, string.Empty);

            var result = new VersionsModel(name);
            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var interim = new List<Version>();
            foreach (var version in element.Descendants("Version"))
            {
                var aliasA = version.Attribute("Alias");
                var alias = aliasA != null ? aliasA.Value : "";
                interim.Add(new Version(version.Attribute("Name").Value, alias, version.Attribute("Status").Value));
            }
            result.Versions.AddRange(interim.OrderBy(v => v.SemVar));
            return result;
        }

        public async Task<string[]> UpdateControllerVersionStatuses(List<VersionUpdate> versionStatusUpdates)
        {
            var result = new List<string>();
            foreach (var item in versionStatusUpdates.OrderBy(s => s.Status))
            {
                _logger.InfoFormat("Sending instruction to update {0}/{1} to status {2} with alias '{3}'",
                    item.Controller, item.Version, item.Status, item.Alias);
                var query = string.Format("controllers/{0}/versions/{1}", item.Controller, item.Version);
                var content = string.Format("{0}|{1}", item.Status, item.Alias);
                var response = await _gateway.Put<string, string>("Catalogue", query, content);

                if (response.Successfull)
                    result.Add(string.Format("Successfully updated version {0} to {1} {2}", item.Version, item.Status,
                        item.Alias));
                else
                    result.Add(string.Format("Failed to update version {0} to {1} {2}: {3}", item.Version, item.Status,
                        item.Alias, response.Message));

                _logger.InfoFormat("Response for update (success={0}): {1}", response.Successfull, response.Message);
            }
            return result.ToArray();
        }

        public async Task<ConfigurationModel> GetControllerConfiguration(string name)
        {
            var response = await _gateway.GetSync("Catalogue", string.Format("controllers/{0}", name), string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            foreach (var item in element.Descendants("Controller"))
            {
                if (item.Attributes().Any(x => x.Name == "Name") && item.Attribute("Name").Value.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    return item.Deserialize<ConfigurationModel>();
            }

            return null;
        }

        public async Task<GatewayResponse<string>> UpdateControllerConfiguration(ConfigurationModel model)
        {
            switch (model.ScalingStrategyId)
            {
                case ConfigurationModel.ScalingStrategies.Automatic:
                    model.PriorityLimits?.Clear();
                    break;

                case ConfigurationModel.ScalingStrategies.Fixed:
                    model.MaxInstances = 1;
                    break;

                case ConfigurationModel.ScalingStrategies.Container:
                    model.MaxPriority = 16;
                    model.MaxInstances = 1;
                    model.PriorityLimits?.Clear();
                    break;
            }

            var query = "/controllers/configuration";
            return await _gateway.Put<string, string>("Catalogue", query, model.Serialize());
        }

        public async Task<GroupsModel> GetGroups()
        {
            var response = await _gateway.GetSync("Security", "groups", string.Empty);

            var result = new GroupsModel();
            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var interim = new List<GroupModel>();
            foreach (var item in element.Descendants("Group"))
            {
                interim.Add(item.Deserialize<GroupModel>());
            }
            result.Items.AddRange(interim.OrderBy(v => v.Name));

            return result;
        }

        public async Task<ReportsModel> GetSecurityReport(string name)
        {
            var query = string.Format("reports/{0}", name);
            var response = await _gateway.GetSync("Security", query, string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            try
            {
                var element = response.Body;
                var model = element.Deserialize<ReportsModel>();
                model.Name = name;
                return model;
            }
            catch (Exception ex)
            {
                _logger.WarnFormat("Could not fetch report: ", ex.Message);
            }

            var result = new ReportsModel();
            result.Name = name;
            return result;
        }

        public async Task<ReportsModel> GetSecurityReport(string name, string parameterName, string parameter)
        {
            if (!string.IsNullOrEmpty(parameter))
            {
                var query = string.Format("reports/{0}/{1}", name, parameter);
                var response = await _gateway.GetSync("Security", query, string.Empty);

                if (!response.Successfull)
                    throw new RemoteGatewayException(response.Message);

                try
                {
                    var element = response.Body;
                    var model = element.Deserialize<ReportsModel>();
                    model.Name = name;
                    model.SupportsParameter = true;
                    model.ParameterName = parameterName;
                    model.Parameter = parameter;
                    return model;
                }
                catch (Exception ex)
                {
                    _logger.WarnFormat("Could not fetch report: ", ex.Message);
                }
            }

            var result = new ReportsModel();
            result.Name = name;
            result.SupportsParameter = true;
            result.ParameterName = parameterName;
            result.Parameter = parameter;
            return result;
        }

        public async Task<GroupModel> GetGroup(long id)
        {
            var query = string.Format("groups/{0}", id);
            var response = await _gateway.GetSync("Security", query, string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var result = element.Deserialize<GroupModel>();
            return result;
        }

        public async Task<string[]> Create(GroupModel model)
        {
            var query = "groups";
            var response = await _gateway.Put<string, string>("Security", query, model.Serialize());

            return SuccessOrMessage(response);
        }

        public async Task<string[]> DeleteGroup(long id)
        {
            var query = string.Format("groups/{0}", id);
            var response = await _gateway.DeleteSync("Security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<UsersModel> GetUsers()
        {
            var response = await _gateway.GetSync("Security", "users", string.Empty);

            var result = new UsersModel();
            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var interim = new List<UserModel>();
            foreach (var item in element.Descendants("User"))
            {
                interim.Add(item.Deserialize<UserModel>());
            }
            result.Items.AddRange(interim.OrderBy(v => v.FullName));

            return result;
        }

        public async Task<UserModel> GetUser(string id)
        {
            var query = string.Format("Users/{0}", id);
            var response = await _gateway.GetSync("Security", query, string.Empty);

            if (!response.Successfull)
                return new UserModel();

            var element = response.Body;
            var result = element.Deserialize<UserModel>();

            return result;
        }

        public async Task<UserModel> GetNonUser(string domain, string login)
        {
            var query = string.Format("Users/{0}", login);
            var response = await _gateway.GetSync("Security", query, string.Empty);

            if (!response.Successfull)
                return new UserModel();

            var element = response.Body;
            var result = element.Deserialize<UserModel>();

            return result;
        }

        public async Task<UserModel> GetUserGroups(long id)
        {
            var query = string.Format("Users/{0}/Groups", id);
            var response = await _gateway.GetSync("Security", query, string.Empty);

            if (!response.Successfull)
                return new UserModel(id);

            var element = response.Body;
            var result = element.Deserialize<UserModel>();

            var model = await GetGroups();
            result.Items.AddRange(
                model.Items.Select(item => new SelectListItem { Value = item.Id.ToString(), Text = item.Name }));
            return result;
        }

        public async Task<ApplicationsModel> GetApplications()
        {
            var result = new ApplicationsModel();

            await PopulateAddIns(result);
            await PopulateApplications(result);

            var versions1 = await GetAvailableReferencedAddInVersions();
            var versions2 = await GetAvailableReferencedApplicationVersions();
            var versions3 = await GetAvailableAddInVersions();
            var versions4 = await GetAvailableApplicationVersions();

            // Get active add-ins
            result.From = versions1.Union(versions2).ToList();
            result.To = versions3.Union(versions4).ToList();
            result.ActiveItems = result.From.ConvertToReportTable();

            return result;
        }

        private async Task PopulateAddIns(ApplicationsModel target)
        {
            var response = await _gateway.GetSync("Security", "addins", string.Empty);
            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var interim = new List<AddInModel>();
            foreach (var item in element.Descendants("AddIn"))
            {
                interim.Add(item.Deserialize<AddInModel>());
            }
            target.Items.AddRange(interim.OrderBy(v => v.Name));
        }

        private async Task PopulateApplications(ApplicationsModel target)
        {
            var response = await _gateway.GetSync("Security", "applications", string.Empty);
            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var interim = new List<ApplicationModel>();
            foreach (var item in element.Descendants("Application"))
            {
                interim.Add(item.Deserialize<ApplicationModel>());
            }
            target.Items.AddRange(interim.OrderBy(v => v.Name));
        }

        public async Task<AddInModel> GetAddIn(long id)
        {
            var query = string.Format("addins/{0}", id);
            var response = await _gateway.GetSync("Security", query, string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var result = element.Deserialize<AddInModel>();

            return result;
        }

        public async Task<IEnumerable<ApplicationVersionModel>> GetApplicationVersions()
        {
            var query = "applications/versions";
            var response = await _gateway.GetSync("Security", query, string.Empty);

            var result = new List<ApplicationVersionModel>();
            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            foreach (var model in DeserializeApplicationVersionItems(element).OrderByDescending(v => v.ActualVersion))
            {
                result.Add(model);
            }

            return result;
        }

        public async Task<IEnumerable<AddInVersionModel>> GetAddInVersions()
        {
            var query = "addins/versions";
            var response = await _gateway.GetSync("Security", query, string.Empty);

            var result = new List<AddInVersionModel>();
            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            foreach (var model in DeserializeVersionItems(element).OrderByDescending(v => v.ActualVersion))
            {
                result.Add(model);
            }

            return result;
        }

        public async Task<string[]> Create(ApplicationModel model)
        {
            var query = "applications";
            var response = await _gateway.Put<string, string>("Security", query, model.Serialize());

            return SuccessOrMessage(response);
        }

        public async Task<string[]> Create(AddInModel model)
        {
            var query = "addins";
            var response = await _gateway.Put<string, string>("Security", query, model.Serialize());

            return SuccessOrMessage(response);
        }

        public async Task<string[]> DeleteAddIn(long id)
        {
            var query = $"addins/{id}";
            var response = await _gateway.DeleteSync("Security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<PermissionsModel> GetPermissions()
        {
            var response = await _gateway.GetSync("Security", "permissions", string.Empty);

            var result = new PermissionsModel();
            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var interim = new List<PermissionModel>();
            foreach (var item in element.Descendants("Permission"))
            {
                interim.Add(item.Deserialize<PermissionModel>());
            }
            foreach (var system in interim.GroupBy(v => v.SystemName).OrderBy(v => v.Key))
            {
                var sys = new SystemPermissions(system.Key);
                sys.Items.AddRange(system.OrderBy(v => v.Name));
                result.Items.Add(sys);
            }

            await PopulateAvailableSystems(result);
            return result;
        }

        public async Task<List<PortfolioModel>> GetPortfolios()
        {
            var response = await _gateway.GetSync("Security", "portfolios", string.Empty);
            var result = new List<PortfolioModel>();

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            element.Descendants("Portfolio").ForEach(item => { result.Add(item.Deserialize<PortfolioModel>()); });

            return result;
        }

        public async Task<PermissionModel> GetPermission(long id)
        {
            var query = string.Format("permissions/{0}", id);
            var response = await _gateway.GetSync("Security", query, string.Empty);

            var result = new PermissionModel();
            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            result = element.Deserialize<PermissionModel>();

            return result;
        }

        public async Task<string[]> DeletePermission(long id)
        {
            var query = string.Format("permissions/{1}", id);
            var response = await _gateway.DeleteSync("Security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<string[]> DeleteGroupPortfolio(long id, long groupId)
        {
            var query = $"groups/{groupId}/portfolios/{id}";
            var response = await _gateway.DeleteSync("Security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<string[]> DeleteGroupPermission(long id, long groupId)
        {
            var query = $"groups/{groupId}/permissions/{id}";
            var response = await _gateway.DeleteSync("Security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<string[]> Create(PermissionModel model)
        {
            var query = "permissions";
            var response = await _gateway.Put<string, string>("Security", query, model.Serialize());

            return SuccessOrMessage(response);
        }

        public async Task<string[]> InsertGroupPermission(long groupId, long permissionId)
        {
            var query = $"groups/{groupId}/permissions/{permissionId}";
            var response = await _gateway.Put<string, string>("Security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<ADGroupsModel> GetGroupADGroups(long groupId)
        {
            var query = $"groups/{groupId}/adgroups";
            var response = await _gateway.GetSync("Security", query, string.Empty);

            var result = new ADGroupsModel(groupId);
            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var interim = new List<GroupActiveDirectory>();
            foreach (var item in element.Descendants("GroupAD"))
            {
                interim.Add(item.Deserialize<GroupActiveDirectory>());
            }
            result.Items.AddRange(interim.OrderBy(v => v.Name));

            return result;
        }

        public async Task<string[]> DeleteGroupADGroup(long id, long groupId)
        {
            var query = $"groups/{groupId}/adgroups/{id}";
            var response = await _gateway.DeleteSync("Security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<string[]> Create(GroupActiveDirectory model)
        {
            var query = $"groups/{model.GroupId}/adgroups";
            var payload = model.Serialize();
            var response = await _gateway.Put<string, string>("Security", query, payload);

            return SuccessOrMessage(response);
        }

        public async Task<Models.Group.PermissionsModel> GetGroupPermisions(long groupId)
        {
            var query = $"groups/{groupId}/permissions";
            var response = await _gateway.GetSync("Security", query, string.Empty);

            var result = new Models.Group.PermissionsModel(groupId);
            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var interim = new List<PermissionModel>();
            foreach (var item in element.Descendants("Permission"))
            {
                interim.Add(item.Deserialize<PermissionModel>());
            }
            result.Items.AddRange(interim.OrderBy(v => v.SystemName).ThenBy(v => v.Name));

            await PopulateAvailablePermissions(result);
            return result;
        }

        public async Task<PortfoliosModel> GetGroupPortfolios(long groupId)
        {
            var query = $"groups/{groupId}/portfolios";
            var response = await _gateway.GetSync("Security", query, string.Empty);

            var result = new PortfoliosModel(groupId);
            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var interim = new List<PortfolioModel>();
            foreach (var item in element.Descendants("Portfolio"))
            {
                interim.Add(item.Deserialize<PortfolioModel>());
            }
            result.Items.AddRange(interim.OrderBy(v => v.Level).ThenBy(v => v.Name));

            return result;
        }

        public async Task<SitesModel> GetGroupSites(long groupId)
        {
            var query = $"groups/{groupId}/sites";
            var response = await _gateway.GetSync("Security", query, string.Empty);

            var result = new SitesModel(groupId);
            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var interim = new List<SiteModel>();
            foreach (var item in element.Descendants("Site"))
            {
                interim.Add(item.Deserialize<SiteModel>());
            }
            result.Items.AddRange(interim.OrderBy(v => v.Name));

            await PopulateAvailableSites(result);
            return result;
        }

        public async Task<Models.Group.UsersModel> GetGroupUsers(long groupId)
        {
            var query = $"groups/{groupId}/users";
            var response = await _gateway.GetSync("Security", query, string.Empty);

            var result = new Models.Group.UsersModel(groupId);
            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var interim = new List<UserModel>();
            foreach (var item in element.Descendants("User"))
            {
                interim.Add(item.Deserialize<UserModel>());
            }
            result.Items.AddRange(interim.OrderBy(v => v.FullName));

            return result;
        }

        public async Task<string[]> DeleteGroupSite(long id, long groupId)
        {
            var query = $"groups/{groupId}/sites/{id}";
            var response = await _gateway.DeleteSync("Security", query, String.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<string[]> InsertGroupSite(long groupId, long siteId)
        {
            var query = $"groups/{groupId}/sites/{siteId}";
            var response = await _gateway.Put<string, string>("Security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<Models.Group.AddInsModel> GetGroupAddIns(long groupId)
        {
            var result = new Models.Group.AddInsModel(groupId);

            await PopulateAddInVersions(result, groupId);
            await PopulateApplicationVersions(result, groupId);

            var versions1 = await GetAvailableAddInVersions();
            var versions2 = await GetAvailableApplicationVersions();

            result.AvailableAddInVersions.AddRange(versions1.OrderBy(v => v.Text));
            result.AvailableApplicationVersions.AddRange(versions2.OrderBy(v => v.Text));

            return result;
        }

        private async Task PopulateAddInVersions(Models.Group.AddInsModel target, long groupId)
        {
            var query = $"groups/{groupId}/addins";
            var response = await _gateway.GetSync("Security", query, string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var interim = new List<GroupAddInVersionModel>();
            foreach (var item in element.Descendants("GroupAddInVersion"))
            {
                interim.Add(item.Deserialize<GroupAddInVersionModel>());
            }
            target.AddIns.AddRange(interim.OrderBy(v => v.Name));
        }

        private async Task PopulateApplicationVersions(Models.Group.AddInsModel target, long groupId)
        {
            var query = $"groups/{groupId}/applications";
            var response = await _gateway.GetSync("Security", query, string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var interim = new List<GroupApplicationVersionModel>();
            foreach (var item in element.Descendants("GroupApplicationVersion"))
            {
                interim.Add(item.Deserialize<GroupApplicationVersionModel>());
            }
            target.Applications.AddRange(interim.OrderBy(v => v.Name));
        }

        public async Task<string[]> InsertGroupAddInVersion(long groupId, AddInVersionModel addInVersion)
        {
            var query = $"groups/{groupId}/addins";
            var response = await _gateway.Put<string, string>("Security", query, addInVersion.Serialize());

            return SuccessOrMessage(response);
        }

        public async Task<string[]> InsertGroupApplicationVersion(long groupId, ApplicationVersionModel addInVersion)
        {
            var query = $"groups/{groupId}/applications";
            var response = await _gateway.Put<string, string>("Security", query, addInVersion.Serialize());

            return SuccessOrMessage(response);
        }

        public async Task<string[]> DeleteGroupAddInVersion(long id, long groupId)
        {
            var query = $"groups/{groupId}/addins/{id}";
            var response = await _gateway.DeleteSync("Security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<string[]> DeleteGroupApplicationVersion(long id, long groupId)
        {
            var query = $"groups/{groupId}/applications/{id}";
            var response = await _gateway.DeleteSync("Security", query, String.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<string[]> UpdateAssignedApplicationVersions(string @from, string to)
        {
            var query = "applications/reassign";
            var payload = $"{@from}|{@to}";
            var response = await _gateway.Put<string, string>("Security", query, payload);

            return SuccessOrMessage(response);
        }

        public async Task<string[]> UpdateAssignedAddInVersions(string @from, string to)
        {
            var query = "addins/reassign";
            var payload = $"{@from}|{@to}";
            var response = await _gateway.Put<string, string>("Security", query, payload);

            return SuccessOrMessage(response);
        }

        public async Task<string[]> RemoveUser(long id)
        {
            var query = $"users/{id}";
            var response = await _gateway.DeleteSync("Security", query, String.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<string[]> Create(UserModel model)
        {
            var response = await _gateway.Put<string, string>("Security", "users", model.Serialize());

            return SuccessOrMessage(response);
        }

        public async Task<string[]> InsertUserPortfolio(long userId, long portfolioId)
        {
            var query = $"users/{userId}/portfolios/{portfolioId}";
            var response = await _gateway.Put<string, string>("Security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<Models.User.PortfoliosModel> GetUserPortfolios(long userId)
        {
            var query = $"users/{userId}/portfolios";
            var response = await _gateway.GetSync("Security", query, string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var model = element.Deserialize<Models.User.PortfoliosModel>();

            var portfolios = await GetPortfolios();

            model.AvailablePortfolios.AddRange(
                portfolios.Select(item => new SelectListItem { Value = item.Id.ToString(), Text = item.Name }));

            return model;
        }

        public async Task<string[]> RemoveUserPortfolio(long userId, long portfolioId)
        {
            var query = $"users/{userId}/portfolios/{portfolioId}";
            var response = await _gateway.DeleteSync("Security", query, String.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<string[]> InsertUserSite(long userId, long siteId)
        {
            var query = $"users/{userId}/sites/{siteId}";
            var response = await _gateway.Put<string, string>("Security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<Models.User.SitesModel> GetUserSites(long userId)
        {
            var query = $"users/{userId}/sites";
            var response = await _gateway.GetSync("Security", query, string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var model = element.Deserialize<Models.User.SitesModel>();

            var sites = await GetSites();
            model.AvailableSites.AddRange(
                sites.Select(item => new SelectListItem { Value = item.Id.ToString(), Text = item.Name }));

            return model;
        }

        public async Task<string[]> RemoveUserSite(long userId, long siteId)
        {
            var query = string.Format("users/{0}/sites/{1}", userId, siteId);
            var response = await _gateway.DeleteSync("Security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<string[]> InsertUserGroup(long userId, long groupId)
        {
            var query = string.Format("users/{0}/groups/{1}", userId, groupId);
            var response = await _gateway.Put<string, string>("Security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<string[]> RemoveUserGroup(long userId, long groupId)
        {
            var query = string.Format("users/{0}/groups/{1}", userId, groupId);
            var response = await _gateway.DeleteSync("Security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<Models.User.AddInsModel> GetUserAddInVersions(long userId)
        {
            var result = new Models.User.AddInsModel(userId);

            await PopulateAddInVersions(result, userId);
            await PopulateApplicationVersions(result, userId);

            var versions1 = await GetAvailableAddInVersions();
            var versions2 = await GetAvailableApplicationVersions();

            result.AvailableAddInVersions.AddRange(versions1.OrderBy(v => v.Text));
            result.AvailableApplicationVersions.AddRange(versions2.OrderBy(v => v.Text));

            return result;
        }

        private async Task PopulateAddInVersions(Models.User.AddInsModel target, long userId)
        {
            var query = string.Format("users/{0}/addins", userId);
            var response = await _gateway.GetSync("Security", query, string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            target.Login = element.Descendants("Login").First().Value;

            var interim = new List<UserAddInVersionModel>();
            foreach (var item in element.Descendants("UserAddInVersion"))
            {
                interim.Add(item.Deserialize<UserAddInVersionModel>());
            }
            target.AddIns.AddRange(interim.OrderBy(v => v.Name));

            var groupValues = new List<GroupAddInVersionModel>();
            foreach (var item in element.Descendants("GroupAddInVersion"))
            {
                groupValues.Add(item.Deserialize<GroupAddInVersionModel>());
            }
            target.GroupExcelAddInVersions.AddRange(groupValues.OrderBy(v => v.Name));
        }

        private async Task PopulateApplicationVersions(Models.User.AddInsModel target, long userId)
        {
            var query = string.Format("users/{0}/applications", userId);
            var response = await _gateway.GetSync("Security", query, string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var interim = new List<UserApplicationVersionModel>();
            foreach (var item in element.Descendants("UserApplicationVersion"))
            {
                interim.Add(item.Deserialize<UserApplicationVersionModel>());
            }
            target.Applications.AddRange(interim.OrderBy(v => v.Name));

            var groupValues = new List<GroupApplicationVersionModel>();
            foreach (var item in element.Descendants("GroupApplicationVersion"))
            {
                groupValues.Add(item.Deserialize<GroupApplicationVersionModel>());
            }
            target.GroupApplicationVersions.AddRange(groupValues.OrderBy(v => v.Name));
        }

        public async Task<string[]> DeleteUserAddInVersions(long userId, long addInVersionId)
        {
            var query = string.Format("users/{0}/addins/{1}", userId, addInVersionId);
            var response = await _gateway.DeleteSync("Security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<string[]> DeleteUserApplicationVersions(long userId, long applicationVersionId)
        {
            var query = string.Format("users/{0}/applications/{1}", userId, applicationVersionId);
            var response = await _gateway.DeleteSync("Security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<string[]> InsertUserAddInVersions(long groupId, AddInVersionModel addInVersion)
        {
            var query = string.Format("users/{0}/addins", groupId);
            var response = await _gateway.Put<string, string>("Security", query, addInVersion.Serialize());

            return SuccessOrMessage(response);
        }

        public async Task<string[]> InsertUserApplicationVersions(long groupId, ApplicationVersionModel version)
        {
            var query = string.Format("users/{0}/applications", groupId);
            var response = await _gateway.Put<string, string>("Security", query, version.Serialize());

            return SuccessOrMessage(response);
        }

        public async Task<IEnumerable<BusinessFunction>> GetBusinessFunctions()
        {
            var query = "businessfunctions";
            var response = await _gateway.GetSync("security", query, string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;

            return element.Descendants("BusinessFunction")
                .Select(item => item.Deserialize<BusinessFunction>())
                .ToList();
        }

        public async Task<string[]> Create(BusinessFunction model)
        {
            var query = "businessfunctions";
            var response = await _gateway.Post<string, string>("security", query, model.Serialize());

            return SuccessOrMessage(response);
        }

        public async Task<string[]> DeleteBusinessFunction(int id)
        {
            var query = string.Format("businessfunctions/{0}", id);
            var response = await _gateway.DeleteSync("security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<IEnumerable<GroupType>> GetGroupTypes()
        {
            var query = "grouptypes";
            var response = await _gateway.GetSync("security", query, string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;

            return element.Descendants("GroupType")
                .Select(item => item.Deserialize<GroupType>())
                .ToList();
        }

        public async Task<string[]> Create(GroupType model)
        {
            var query = "grouptypes";
            var response = await _gateway.Post<String, string>("security", query, model.Serialize());

            return SuccessOrMessage(response);
        }

        public async Task<string[]> DeleteGroupType(int id)
        {
            var query = string.Format("grouptypes/{0}", id);
            var response = await _gateway.DeleteSync("security", query, string.Empty);

            return SuccessOrMessage(response);
        }

        public async Task<bool> GenerateDocumentation(string id, string version)
        {
            var query = string.Format("controllers/{0}/versions/{1}/documentation", id, version);
            var response = await _gateway.Put<string, string>("Catalogue", query, string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            return true;
        }

        public async Task<string[]> UpdateGroupBusinessFunction(string groupId, string businessFunctionId)
        {
            var query = string.Format("groups/{0}/businessfunction/{1}", groupId, businessFunctionId);
            var response = await _gateway.Put<string, string>("security", query, null);

            return SuccessOrMessage(response);
        }

        private string GetDefaultKnownGateways(string environment)
        {
            if (environment.Equals("PROD", StringComparison.InvariantCultureIgnoreCase))
                return "sigma.absa.co.za";

            return $"sigma-{environment}.absa.co.za";
        }

        private IEnumerable<QueueModel> GetCurrentQueues(IEnumerable<ServerResponse> docs)
        {
            foreach (var doc in docs)
            {
                if (doc.Document == null) continue;

                foreach (var info in doc.Document.Descendants("Controller"))
                {
                    var item = info.Deserialize<ControllerQueueInformation>();
                    foreach (var version in item.Versions)
                    {
                        foreach (var queue in version.Queues)
                        {
                            var result = new QueueModel
                            {
                                Server = doc.Server,
                                Controller = item.Name,
                                Version = version.Name,
                                Length = queue.Length,
                                Workers = queue.Workers
                            };
                            DateTime t;
                            if (DateTime.TryParse(queue.LastEnqueue, out t))
                                result.LastEnqueue = t;
                            if (DateTime.TryParse(queue.LastEnqueue, out t))
                                result.LastDequeue = t;

                            yield return result;
                        }
                    }
                }
            }
        }

        private async Task<ServerResponse> Fetch(string query, params string[] args)
        {
            var url = args != null && args.Length > 0 ? string.Format(query, args) : query;

            foreach (var gateway in _gateways)
            {
                var document = await Get(gateway, url);
                return new ServerResponse(gateway, document);
            }
            return null;
        }

        private async Task<IEnumerable<ServerResponse>> FetchAll(string query, params string[] args)
        {
            var url = args != null && args.Length > 0 ? string.Format(query, args) : query;
            var results = new List<ServerResponse>();
            //TODO: Update gateway endpoints to return results for all gateways then remove this method.
            foreach (var gateway in _gateways)
            {
                var document = await Get(gateway, url);
                if (document != null)
                    results.Add(new ServerResponse(gateway, document));
            }

            return results;
        }

        private async Task<XDocument> Get(string gateway, string query)
        {
            var uri = string.Format("https://{0}:{1}/{2}", gateway, _port, query);

            using (var client = new HttpClient(new HttpClientHandler
            {
                UseDefaultCredentials = true,
                AllowAutoRedirect = true
            }))
            {
                client.DefaultRequestHeaders.Add("Accept", "application/xml");

                client.Timeout = _defaultRequestTimeout;
                var response = await client.GetAsync(uri);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new RemoteGatewayException(response.ReasonPhrase);
                }

                var responseContent = response.Content.ReadAsStringAsync();
                responseContent.Wait();

                return XDocument.Parse(responseContent.Result);
            }
        }

        public async Task<string> GetAsync(string gateway, string query)
        {
            var uri = $"https://{gateway}:{_port}/{query}";
            var client = _defaultClient.Value;
            using (var response = await client.GetAsync(uri))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new RemoteGatewayException(response.ReasonPhrase);
                }

                return await response.Content.ReadAsStringAsync();
            }
        }

        private async Task Delete(string query)
        {
            var gateway = _gateways.FirstOrDefault();

            var url = string.Format("https://{0}:{1}/{2}", gateway, _port, query);

            using (var client = new HttpClient(new HttpClientHandler
            {
                UseDefaultCredentials = true,
                AllowAutoRedirect = true
            }))
            {
                client.Timeout = _defaultRequestTimeout;
                var response = await client.DeleteAsync(url);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task RequestWorkersAsync(RequestedWorkers requestedWorkers)
        {
            var gateway = _gateways.FirstOrDefault();
            var query = $"worker/requestworker/{requestedWorkers.ControllerName}/{requestedWorkers.Version}/{requestedWorkers.Priority}/{requestedWorkers.Instances}";
            var url = string.Format("https://{0}:{1}/{2}", gateway, _port, query);

            using (var client = new HttpClient(new HttpClientHandler
            {
                UseDefaultCredentials = true,
                AllowAutoRedirect = true
            }))
            {
                client.Timeout = _defaultRequestTimeout;
                var response = await client.PostAsync(url, null);
            }
        }

        private async Task PopulateAvailableSystems(PermissionsModel target)
        {
            var response = await _gateway.GetSync("Security", "systems", string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var interim = new List<SelectListItem>();
            foreach (var item in element.Descendants("SystemName"))
            {
                var model = item.Deserialize<SystemNameModel>();
                var foo = new SelectListItem { Text = model.Name, Value = model.Id.ToString() };
                interim.Add(foo);
            }

            target.AvailableSystems.AddRange(interim.OrderBy(v => v.Text));
        }

        private async Task PopulateAvailablePermissions(Models.Group.PermissionsModel target)
        {
            var response = await _gateway.GetSync("Security", "permissions", string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var interim = new List<SelectListItem>();
            foreach (var item in element.Descendants("Permission"))
            {
                var model = item.Deserialize<PermissionModel>();
                var foo = new SelectListItem
                {
                    Text = $"{model.SystemName} - {model.Name}",
                    Value = model.Id.ToString()
                };
                interim.Add(foo);
            }

            target.AvailablePermissions.AddRange(interim.OrderBy(v => v.Text));
        }

        private async Task PopulateAvailableSites(SitesModel target)
        {
            var response = await _gateway.GetSync("Security", "sites", string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            var interim = new List<SelectListItem>();
            foreach (var item in element.Descendants("Site"))
            {
                var model = item.Deserialize<SiteModel>();
                var foo = new SelectListItem { Text = model.Name, Value = model.Id.ToString() };
                interim.Add(foo);
            }

            target.AvailableSites.AddRange(interim.OrderBy(v => v.Text));
        }

        private async Task<IEnumerable<SelectListItem>> GetAvailableApplicationVersions()
        {
            var response = await _gateway.GetSync("Security", "applications/versions", string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            return DeserializeApplicationVersionItems(element)
                .OrderByDescending(v => v.ActualVersion)
                .Select(model => new SelectListItem
                {
                    Text = $"{model.Application} - {model.Version}",
                    Value = $"{model.Application}|{model.Version}|Application"
                }).ToList();
        }

        private async Task<IEnumerable<SelectListItem>> GetAvailableReferencedApplicationVersions()
        {
            var response = await _gateway.GetSync("Security", "applications/versions/referenced", string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            return DeserializeApplicationVersionItems(element)
                .OrderByDescending(v => v.ActualVersion)
                .Select(model => new SelectListItem
                {
                    Text = $"{model.Application} - {model.Version}",
                    Value = $"{model.Application}|{model.Version}"
                }).ToList();
        }

        private async Task<IEnumerable<SelectListItem>> GetAvailableAddInVersions()
        {
            var response = await _gateway.GetSync("Security", "addins/versions", string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;

            return DeserializeVersionItems(element)
                .OrderByDescending(v => v.ActualVersion)
                .Select(model => new SelectListItem
                {
                    Text = $"{model.AddIn} - {model.Version}",
                    Value = $"{model.AddIn}|{model.Version}|Add-In"
                }).ToList();
        }

        private async Task<IEnumerable<SelectListItem>> GetAvailableReferencedAddInVersions()
        {
            var response = await _gateway.GetSync("Security", "addins/versions/referenced", string.Empty);

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body;
            return DeserializeVersionItems(element)
                .OrderByDescending(v => v.ActualVersion)
                .Select(model => new SelectListItem
                {
                    Text = $"{model.AddIn} - {model.Version}",
                    Value = $"{model.AddIn}|{model.Version}"
                }).ToList();
        }

        private IEnumerable<AddInVersionModel> DeserializeVersionItems(XElement element)
        {
            foreach (var item in element.Descendants("AddInVersion"))
            {
                var model = item.Deserialize<AddInVersionModel>();
                if (string.Equals(model.AddIn, "ExcelTools", StringComparison.CurrentCultureIgnoreCase)) continue;
                yield return model;
            }
        }

        private IEnumerable<ApplicationVersionModel> DeserializeApplicationVersionItems(XElement element)
        {
            foreach (var item in element.Descendants("ApplicationVersion"))
            {
                var model = item.Deserialize<ApplicationVersionModel>();
                if (string.Equals(model.Application, "ExcelTools", StringComparison.CurrentCultureIgnoreCase)) continue;
                yield return model;
            }
        }

        private async Task<GatewayInfo> GetGatewayInfo()
        {
            var serverResponse = await Fetch("health/info");

            if (serverResponse == null ||
                serverResponse.Document == null ||
                !serverResponse.Document.Descendants().Any())
                return new GatewayInfo();

            var xmlElement = serverResponse.Document.Descendants().First();
            return xmlElement.Deserialize<GatewayInfo>();
        }

        public async Task<List<MonikerCheckResult>> GetMonikers(string server, string query)
        {
            var response = await _gateway.GetSync("marketdata", query, string.Empty);

            var result = new List<MonikerCheckResult>();

            if (!response.Successfull)
                throw new RemoteGatewayException(response.Message);

            var element = response.Body.ToString().Deserialize<MarketDataResponse>();

            result.AddRange(element.VerifiedMonikersResult.Failures);
            result.AddRange(element.VerifiedMonikersResult.Successes);

            return result;
        }

        //Needs to be reworked.
        public async Task CancelWorkItemAsync(string correlationId)
        {
            var gateway = _gateways.FirstOrDefault();
            await GetAsync(gateway, $"worker/cancel/{correlationId}");
        }

        public async Task RetryWorkItemAsync(string correlationId)
        {
            var gateway = _gateways.FirstOrDefault();
            await GetAsync(gateway, $"worker/retry/{correlationId}");
        }

        public async Task KillWorkersAsync()
        {
            await Delete($"worker/kill/all");
        }

        public async Task KillWorkersAsync(string controller)
        {
            await Delete($"worker/kill/controllers/{controller}");
        }

        public async Task ShutdownWorkersAsync(string controller)
        {
            await Delete($"worker/shutdown/{controller}");
        }

        public async Task ShutdownWorkerAsync(string queuename, string id)
        {
            await Post($"worker/shutdown/{id}", queuename);
        }

        private async Task Post(string query, string content)
        {
            var gateway = _gateways.FirstOrDefault();

            var url = string.Format("https://{0}:{1}/{2}", gateway, _port, query);

            using (var client = new HttpClient(new HttpClientHandler
            {
                UseDefaultCredentials = true,
                AllowAutoRedirect = true
            }))
            {
                client.Timeout = _defaultRequestTimeout;
                var response = await client.PostAsync(url, new StringContent(content));
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task KillWorkerAsync(string queuename, string id)
        {
            await Post($"worker/kill/{id}", queuename);
        }

        private class ServerResponse
        {
            public ServerResponse(string server, XDocument document)
            {
                Server = server;
                Document = document;
            }

            public string Server { get; }

            public XDocument Document { get; }
        }

        private string[] SuccessOrMessage(GatewayResponse<string> response)
        {
            // Return null as this is considered success.
            if (response.Successfull)
            {
                return null;
            }

            // Return the error message (either in body or message).
            return new[] { response.Body ?? response.Message };
        }

        public void NotifyResourceUpdate()
        {
            var gateway = _gateways.FirstOrDefault();

            var uri = string.Format("https://{0}:{1}/{2}", gateway, _port, "resources/notify");

            using (var client = new HttpClient(new HttpClientHandler
            {
                UseDefaultCredentials = true,
                AllowAutoRedirect = true
            }))
            {
                client.DefaultRequestHeaders.Add("Accept", "application/xml");

                client.Timeout = _defaultRequestTimeout;
                var response = client.GetAsync(uri);
                response.Wait(_defaultRequestTimeout);

                if (response.Result.StatusCode != HttpStatusCode.OK)
                {
                    throw new RemoteGatewayException(response.Result.ReasonPhrase);
                }
            }
        }

        public async Task<GatewayResponse<TResponse>> Send<TResponse>(GatewayRequest request)
        {
            return await _gateway.Invoke<TResponse>(request);
        }
    }

    public class RemoteGatewayException : Exception
    {
        public RemoteGatewayException(string message)
            : base(message)
        {
        }
    }
}