﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Linq;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models.AddIn;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Models.Group;
using Gateway.Web.Models.Permission;
using Gateway.Web.Models.Request;
using Gateway.Web.Models.Security;
using Gateway.Web.Models.Shared;
using Gateway.Web.Models.User;
using Gateway.Web.Utils;
using WebGrease.Css.Extensions;
using ApplicationsModel = Gateway.Web.Models.Security.ApplicationsModel;
using GroupsModel = Gateway.Web.Models.Security.GroupsModel;
using PermissionsModel = Gateway.Web.Models.Security.PermissionsModel;
using PortfoliosModel = Gateway.Web.Models.Group.PortfoliosModel;
using SitesModel = Gateway.Web.Models.Group.SitesModel;
using UsersModel = Gateway.Web.Models.Security.UsersModel;
using Version = Gateway.Web.Models.Controller.Version;
using VersionsModel = Gateway.Web.Models.Controller.VersionsModel;

namespace Gateway.Web.Services
{
    public class GatewayService : IGatewayService
    {
        private readonly TimeSpan _defaultRequestTimeout;
        private readonly IGatewayRestService _gatewayRestService;
        private readonly string[] _gateways;
        private readonly ILogger _logger;
        private readonly int _port = 7010;

        public GatewayService(
            ISystemInformation information,
            IGatewayRestService gatewayRestService,
            ILoggingService loggingService
            )
        {
            _defaultRequestTimeout = TimeSpan.FromSeconds(300);
            _gatewayRestService = gatewayRestService;
            _logger = loggingService.GetLogger(this);
            var gateways = information.GetSetting("KnownGateways", GetDefaultKnownGateways(information.EnvironmentName));
            _gateways = gateways.Split(';');
        }

        public ServersModel GetServers()
        {
            var gatewayInfo = GetGatewayInfo();
            return new ServersModel(gatewayInfo);
        }

        public WorkersModel GetWorkers()
        {
            var gatewayInfo = GetGatewayInfo();
            return new WorkersModel().BuildModel(gatewayInfo);
        }

        public WorkersModel GetWorkers(string controller)
        {
            var response = Fetch("health/info");

            if (response == null || !response.Document.Descendants().Any())
                return new WorkersModel(controller);

            var xmlElement = response.Document.Descendants().First();
            var gatewayInfo = xmlElement.Deserialize<GatewayInfo>();
            return new WorkersModel(controller).BuildModel(gatewayInfo);
        }

        public IEnumerable<QueueModel> GetCurrentQueues(string controller)
        {
            var docs = FetchAll("health/queues/{0}", controller);
            return GetCurrentQueues(docs);
        }

        public IEnumerable<QueueModel> GetCurrentQueues()
        {
            var docs = FetchAll("health/queues");
            return GetCurrentQueues(docs);
        }

        public XElement[] GetReport(string report)
        {
            var doc = Fetch("api/riskdata/latest/{0}", report);

            var element = doc.Document.Descendants("xVAReturnResult").ToArray();
            return element;
        }

        public List<SiteModel> GetSites()
        {
            var response = _gatewayRestService.Get("Security", "latest", "sites");
            var result = new List<SiteModel>();

            if (!response.Successfull)
                return result;

            var element = response.Content.GetPayloadAsXElement();
            element.Descendants("Site").ForEach(item => { result.Add(item.Deserialize<SiteModel>()); });

            return result;
        }

        public void ExpireWorkItem(string id)
        {
            var result = Delete("health/queueitem/{0}", id);
            result.Wait(2000);
        }

        public VersionsModel GetControllerVersions(string name)
        {
            var query = string.Format("controllers/{0}", name);
            var response = _gatewayRestService.Get("Catalogue", "latest", query);

            var result = new VersionsModel(name);
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<Version>();
                foreach (var version in element.Descendants("Version"))
                {
                    var aliasA = version.Attribute("Alias");
                    var alias = aliasA != null ? aliasA.Value : "";
                    interim.Add(new Version(version.Attribute("Name").Value, alias, version.Attribute("Status").Value));
                }
                result.Versions.AddRange(interim.OrderBy(v => v.SemVar));
            }

            return result;
        }

        public string[] UpdateControllerVersionStatuses(List<VersionUpdate> versionStatusUpdates)
        {
            var result = new List<string>();
            foreach (var item in versionStatusUpdates.OrderBy(s => s.Status))
            {
                _logger.InfoFormat("Sending instruction to update {0}/{1} to status {2} with alias '{3}'",
                    item.Controller, item.Version, item.Status, item.Alias);
                var query = string.Format("controllers/{0}/versions/{1}", item.Controller, item.Version);
                var content = string.Format("{0}|{1}", item.Status, item.Alias);
                var response = _gatewayRestService.Put("Catalogue", "latest", query, content);

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

        public RequestPayload GetRequestTree(Guid correlationId)
        {
            var response = _gatewayRestService.Get("Catalogue", string.Format("tree/{0}", correlationId),
                CancellationToken.None);
            if (!response.Successfull)
                return new RequestPayload { ChildRequests = new ChildRequests() };

            return response.Content.GetPayloadAsXElement().DeserializeUsingDataContract<RequestPayload>();
        }

        public ConfigurationModel GetControllerConfiguration(string name)
        {
            var response = _gatewayRestService.Get("Catalogue", string.Format("controllers/{0}", name),
                CancellationToken.None);

            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();

                foreach (var item in element.Descendants("Controller"))
                {
                    if (item.Attributes().Any(x => x.Name == "Name") && item.Attribute("Name").Value.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                        return item.Deserialize<ConfigurationModel>();
                }
            }

            return null;
        }

        public RestResponse UpdateControllerConfiguration(ConfigurationModel model)
        {
            var query = "/controllers/configuration";
            return _gatewayRestService.Put("Catalogue", "latest", query, model.Serialize());
        }

        public GroupsModel GetGroups()
        {
            var response = _gatewayRestService.Get("Security", "latest", "groups");

            var result = new GroupsModel();
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<GroupModel>();
                foreach (var item in element.Descendants("Group"))
                {
                    interim.Add(item.Deserialize<GroupModel>());
                }
                result.Items.AddRange(interim.OrderBy(v => v.Name));
            }

            return result;
        }

        public ReportsModel GetSecurityReport(string name)
        {
            var query = string.Format("reports/{0}", name);
            var response = _gatewayRestService.Get("Security", "latest", query);

            if (response.Successfull)
            {
                try
                {
                    var element = response.Content.GetPayloadAsXElement();
                    var model = element.Deserialize<ReportsModel>();
                    model.Name = name;
                    return model;
                }
                catch (Exception ex)
                {
                    _logger.WarnFormat("Could not fetch report: ", ex.Message);
                }
            }

            var result = new ReportsModel();
            result.Name = name;
            return result;
        }

        public ReportsModel GetSecurityReport(string name, string parameter)
        {
            if (!string.IsNullOrEmpty(parameter))
            {
                var query = string.Format("reports/{0}/{1}", name, parameter);
                var response = _gatewayRestService.Get("Security", "latest", query);

                if (response.Successfull)
                {
                    try
                    {
                        var element = response.Content.GetPayloadAsXElement();
                        var model = element.Deserialize<ReportsModel>();
                        model.Name = name;
                        model.SupportsParameter = true;
                        model.Parameter = parameter;
                        return model;
                    }
                    catch (Exception ex)
                    {
                        _logger.WarnFormat("Could not fetch report: ", ex.Message);
                    }
                }
            }

            var result = new ReportsModel();
            result.Name = name;
            result.SupportsParameter = true;
            result.Parameter = parameter;
            return result;
        }

        public GroupModel GetGroup(long id)
        {
            var query = string.Format("groups/{0}", id);
            var response = _gatewayRestService.Get("Security", "latest", query);

            var result = new GroupModel();
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                result = element.Deserialize<GroupModel>();
            }

            return result;
        }

        public string[] Create(GroupModel model)
        {
            var query = "groups";
            var response = _gatewayRestService.Put("Security", "latest", query, model.Serialize());

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public string[] DeleteGroup(long id)
        {
            var query = string.Format("groups/{0}", id);
            var response = _gatewayRestService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public UsersModel GetUsers()
        {
            var response = _gatewayRestService.Get("Security", "latest", "users");

            var result = new UsersModel();
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<UserModel>();
                foreach (var item in element.Descendants("User"))
                {
                    interim.Add(item.Deserialize<UserModel>());
                }
                result.Items.AddRange(interim.OrderBy(v => v.FullName));
            }

            return result;
        }

        public UserModel GetUser(string id)
        {
            var query = string.Format("Users/{0}", id);
            var response = _gatewayRestService.Get("Security", "latest", query);

            if (!response.Successfull)
                return new UserModel();

            var element = response.Content.GetPayloadAsXElement();
            var result = element.Deserialize<UserModel>();

            return result;
        }

        public UserModel GetNonUser(string domain, string login)
        {
            var query = string.Format("Users/{0}", login);
            var response = _gatewayRestService.Get("Security", "latest", query);

            if (!response.Successfull)
                return new UserModel();

            var element = response.Content.GetPayloadAsXElement();
            var result = element.Deserialize<UserModel>();

            return result;
        }

        public UserModel GetUserGroups(long id)
        {
            var query = string.Format("Users/{0}/Groups", id);
            var response = _gatewayRestService.Get("Security", "latest", query);

            if (!response.Successfull)
                return new UserModel(id);

            var element = response.Content.GetPayloadAsXElement();
            var result = element.Deserialize<UserModel>();

            var model = GetGroups();
            result.Items.AddRange(
                model.Items.Select(item => new SelectListItem { Value = item.Id.ToString(), Text = item.Name }));
            return result;
        }

        public ApplicationsModel GetApplications()
        {
            var result = new ApplicationsModel();

            PopulateAddIns(result);
            PopulateApplications(result);

            // Get active add-ins
            result.From = GetAvailableReferencedAddInVersions().Union(GetAvailableReferencedApplicationVersions()).ToList();
            result.To = GetAvailableAddInVersions().Union(GetAvailableApplicationVersions()).ToList();
            result.ActiveItems = result.From.ConvertToReportTable();

            return result;
        }

        private void PopulateAddIns(ApplicationsModel target)
        {
            var response = _gatewayRestService.Get("Security", "latest", "addins");
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<AddInModel>();
                foreach (var item in element.Descendants("AddIn"))
                {
                    interim.Add(item.Deserialize<AddInModel>());
                }
                target.Items.AddRange(interim.OrderBy(v => v.Name));
            }
        }

        private void PopulateApplications(ApplicationsModel target)
        {
            var response = _gatewayRestService.Get("Security", "latest", "applications");
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<ApplicationModel>();
                foreach (var item in element.Descendants("Application"))
                {
                    interim.Add(item.Deserialize<ApplicationModel>());
                }
                target.Items.AddRange(interim.OrderBy(v => v.Name));
            }
        }

        public AddInModel GetAddIn(long id)
        {
            var query = string.Format("addins/{0}", id);
            var response = _gatewayRestService.Get("Security", "latest", query);

            var result = new AddInModel();
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                result = element.Deserialize<AddInModel>();
            }

            return result;
        }

        public IEnumerable<ApplicationVersionModel> GetApplicationVersions()
        {
            var query = "applications/versions";
            var response = _gatewayRestService.Get("Security", "latest", query);

            var result = new List<ApplicationVersionModel>();
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                foreach (var model in DeserializeApplicationVersionItems(element).OrderByDescending(v => v.ActualVersion))
                {
                    result.Add(model);
                }
            }

            return result;
        }

        public IEnumerable<AddInVersionModel> GetAddInVersions()
        {
            var query = "addins/versions";
            var response = _gatewayRestService.Get("Security", "latest", query);

            var result = new List<AddInVersionModel>();
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                foreach (var model in DeserializeVersionItems(element).OrderByDescending(v => v.ActualVersion))
                {
                    result.Add(model);
                }
            }

            return result;
        }

        public string[] Create(ApplicationModel model)
        {
            var query = "applications";
            var response = _gatewayRestService.Put("Security", "latest", query, model.Serialize());

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public string[] Create(AddInModel model)
        {
            var query = "addins";
            var response = _gatewayRestService.Put("Security", "latest", query, model.Serialize());

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public string[] DeleteAddIn(long id)
        {
            var query = string.Format("addins/{0}", id);
            var response = _gatewayRestService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public PermissionsModel GetPermissions()
        {
            var response = _gatewayRestService.Get("Security", "latest", "permissions");

            var result = new PermissionsModel();
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
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
            }

            PopulateAvailableSystems(result);
            return result;
        }

        public List<PortfolioModel> GetPortfolios()
        {
            var response = _gatewayRestService.Get("Security", "latest", "portfolios");
            var result = new List<PortfolioModel>();

            if (!response.Successfull)
                return result;

            var element = response.Content.GetPayloadAsXElement();
            element.Descendants("Portfolio").ForEach(item => { result.Add(item.Deserialize<PortfolioModel>()); });

            return result;
        }

        public PermissionModel GetPermission(long id)
        {
            var query = string.Format("permissions/{0}", id);
            var response = _gatewayRestService.Get("Security", "latest", query);

            var result = new PermissionModel();
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                result = element.Deserialize<PermissionModel>();
            }

            return result;
        }
        public string[] DeletePermission(long id)
        {
            var query = string.Format("permissions/{1}", id);
            var response = _gatewayRestService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public string[] DeleteGroupPortfolio(long id, long groupId)
        {
            var query = string.Format("groups/{0}/portfolios/{1}", groupId, id);
            var response = _gatewayRestService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public string[] DeleteGroupPermission(long id, long groupId)
        {
            var query = string.Format("groups/{0}/permissions/{1}", groupId, id);
            var response = _gatewayRestService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public string[] Create(PermissionModel model)
        {
            var query = "permissions";
            var response = _gatewayRestService.Put("Security", "latest", query, model.Serialize());

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public string[] InsertGroupPermission(long groupId, long permissionId)
        {
            var query = string.Format("groups/{0}/permissions/{1}", groupId, permissionId);
            var response = _gatewayRestService.Put("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public ADGroupsModel GetGroupADGroups(long groupId)
        {
            var query = string.Format("groups/{0}/adgroups", groupId);
            var response = _gatewayRestService.Get("Security", "latest", query);

            var result = new ADGroupsModel(groupId);
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<GroupActiveDirectory>();
                foreach (var item in element.Descendants("GroupAD"))
                {
                    interim.Add(item.Deserialize<GroupActiveDirectory>());
                }
                result.Items.AddRange(interim.OrderBy(v => v.Name));
            }

            return result;
        }

        public string[] DeleteGroupADGroup(long id, long groupId)
        {
            var query = string.Format("groups/{0}/adgroups/{1}", groupId, id);
            var response = _gatewayRestService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public string[] Create(GroupActiveDirectory model)
        {
            var query = string.Format("groups/{0}/adgroups", model.GroupId);
            var payload = model.Serialize();
            var response = _gatewayRestService.Put("Security", "latest", query, payload);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public Models.Group.PermissionsModel GetGroupPermisions(long groupId)
        {
            var query = string.Format("groups/{0}/permissions", groupId);
            var response = _gatewayRestService.Get("Security", "latest", query);

            var result = new Models.Group.PermissionsModel(groupId);
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<PermissionModel>();
                foreach (var item in element.Descendants("Permission"))
                {
                    interim.Add(item.Deserialize<PermissionModel>());
                }
                result.Items.AddRange(interim.OrderBy(v => v.SystemName).ThenBy(v => v.Name));
            }

            PopulateAvailablePermissions(result);
            return result;
        }

        public PortfoliosModel GetGroupPortfolios(long groupId)
        {
            var query = string.Format("groups/{0}/portfolios", groupId);
            var response = _gatewayRestService.Get("Security", "latest", query);

            var result = new PortfoliosModel(groupId);
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<PortfolioModel>();
                foreach (var item in element.Descendants("Portfolio"))
                {
                    interim.Add(item.Deserialize<PortfolioModel>());
                }
                result.Items.AddRange(interim.OrderBy(v => v.Level).ThenBy(v => v.Name));
            }

            return result;
        }

        public SitesModel GetGroupSites(long groupId)
        {
            var query = string.Format("groups/{0}/sites", groupId);
            var response = _gatewayRestService.Get("Security", "latest", query);

            var result = new SitesModel(groupId);
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<SiteModel>();
                foreach (var item in element.Descendants("Site"))
                {
                    interim.Add(item.Deserialize<SiteModel>());
                }
                result.Items.AddRange(interim.OrderBy(v => v.Name));
            }

            PopulateAvailableSites(result);
            return result;
        }

        public Models.Group.UsersModel GetGroupUsers(long groupId)
        {
            var query = string.Format("groups/{0}/users", groupId);
            var response = _gatewayRestService.Get("Security", "latest", query);

            var result = new Models.Group.UsersModel(groupId);
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<UserModel>();
                foreach (var item in element.Descendants("User"))
                {
                    interim.Add(item.Deserialize<UserModel>());
                }
                result.Items.AddRange(interim.OrderBy(v => v.FullName));
            }
            return result;
        }

        public string[] DeleteGroupSite(long id, long groupId)
        {
            var query = string.Format("groups/{0}/sites/{1}", groupId, id);
            var response = _gatewayRestService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public string[] InsertGroupSite(long groupId, long siteId)
        {
            var query = string.Format("groups/{0}/sites/{1}", groupId, siteId);
            var response = _gatewayRestService.Put("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public Models.Group.AddInsModel GetGroupAddIns(long groupId)
        {
            var result = new Models.Group.AddInsModel(groupId);

            PopulateAddInVersions(result, groupId);
            PopulateApplicationVersions(result, groupId);

            result.AvailableAddInVersions.AddRange(GetAvailableAddInVersions().OrderBy(v => v.Text));
            result.AvailableApplicationVersions.AddRange(GetAvailableApplicationVersions().OrderBy(v => v.Text));

            return result;
        }

        private void PopulateAddInVersions(Models.Group.AddInsModel target, long groupId)
        {
            var query = string.Format("groups/{0}/addins", groupId);
            var response = _gatewayRestService.Get("Security", "latest", query);
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<GroupAddInVersionModel>();
                foreach (var item in element.Descendants("GroupAddInVersion"))
                {
                    interim.Add(item.Deserialize<GroupAddInVersionModel>());
                }
                target.AddIns.AddRange(interim.OrderBy(v => v.Name));
            }
        }

        private void PopulateApplicationVersions(Models.Group.AddInsModel target, long groupId)
        {
            var query = string.Format("groups/{0}/applications", groupId);
            var response = _gatewayRestService.Get("Security", "latest", query);
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<GroupApplicationVersionModel>();
                foreach (var item in element.Descendants("GroupApplicationVersion"))
                {
                    interim.Add(item.Deserialize<GroupApplicationVersionModel>());
                }
                target.Applications.AddRange(interim.OrderBy(v => v.Name));
            }
        }

        public string[] InsertGroupAddInVersion(long groupId, AddInVersionModel addInVersion)
        {
            var query = string.Format("groups/{0}/addins", groupId);
            var response = _gatewayRestService.Put("Security", "latest", query, addInVersion.Serialize());

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public string[] InsertGroupApplicationVersion(long groupId, ApplicationVersionModel addInVersion)
        {
            var query = string.Format("groups/{0}/applications", groupId);
            var response = _gatewayRestService.Put("Security", "latest", query, addInVersion.Serialize());

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public string[] DeleteGroupAddInVersion(long id, long groupId)
        {
            var query = string.Format("groups/{0}/addins/{1}", groupId, id);
            var response = _gatewayRestService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public string[] DeleteGroupApplicationVersion(long id, long groupId)
        {
            var query = string.Format("groups/{0}/applications/{1}", groupId, id);
            var response = _gatewayRestService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public string[] UpdateAssignedApplicationVersions(string @from, string to)
        {
            var query = "applications/reassign";
            var payload = string.Format("{0}|{1}", @from, @to);
            var response = _gatewayRestService.Put("Security", "latest", query, payload);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public string[] UpdateAssignedAddInVersions(string @from, string to)
        {
            var query = "addins/reassign";
            var payload = string.Format("{0}|{1}", @from, @to);
            var response = _gatewayRestService.Put("Security", "latest", query, payload);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public string[] RemoveUser(long id)
        {
            var query = string.Format("users/{0}", id);
            var response = _gatewayRestService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public string[] Create(UserModel model)
        {
            var response = _gatewayRestService.Put("Security", "latest", "users", model.Serialize());

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public string[] InsertUserPortfolio(long userId, long portfolioId)
        {
            var query = string.Format("users/{0}/portfolios/{1}", userId, portfolioId);
            var response = _gatewayRestService.Put("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public Models.User.PortfoliosModel GetUserPortfolios(long userId)
        {
            var query = string.Format("users/{0}/portfolios", userId);
            var response = _gatewayRestService.Get("Security", "latest", query);

            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var model = element.Deserialize<Models.User.PortfoliosModel>();

                var portfolios = GetPortfolios();

                model.AvailablePortfolios.AddRange(
                    portfolios.Select(item => new SelectListItem { Value = item.Id.ToString(), Text = item.Name }));

                return model;
            }

            return null;
        }

        public string[] RemoveUserPortfolio(long userId, long portfolioId)
        {
            var query = string.Format("users/{0}/portfolios/{1}", userId, portfolioId);
            var response = _gatewayRestService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public string[] InsertUserSite(long userId, long siteId)
        {
            var query = string.Format("users/{0}/sites/{1}", userId, siteId);
            var response = _gatewayRestService.Put("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public Models.User.SitesModel GetUserSites(long userId)
        {
            var query = string.Format("users/{0}/sites", userId);
            var response = _gatewayRestService.Get("Security", "latest", query);

            if (!response.Successfull)
                return null;

            var element = response.Content.GetPayloadAsXElement();
            var model = element.Deserialize<Models.User.SitesModel>();

            var sites = GetSites();
            model.AvailableSites.AddRange(
                sites.Select(item => new SelectListItem { Value = item.Id.ToString(), Text = item.Name }));

            return model;
        }

        public string[] RemoveUserSite(long userId, long siteId)
        {
            var query = string.Format("users/{0}/sites/{1}", userId, siteId);
            var response = _gatewayRestService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public string[] InsertUserGroup(long userId, long groupId)
        {
            var query = string.Format("users/{0}/groups/{1}", userId, groupId);
            var response = _gatewayRestService.Put("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public string[] RemoveUserGroup(long userId, long groupId)
        {
            var query = string.Format("users/{0}/groups/{1}", userId, groupId);
            var response = _gatewayRestService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public Models.User.AddInsModel GetUserAddInVersions(long userId)
        {
            var result = new Models.User.AddInsModel(userId);

            PopulateAddInVersions(result, userId);
            PopulateApplicationVersions(result, userId);

            result.AvailableAddInVersions.AddRange(GetAvailableAddInVersions().OrderBy(v => v.Text));
            result.AvailableApplicationVersions.AddRange(GetAvailableApplicationVersions().OrderBy(v => v.Text));

            return result;
        }

        private void PopulateAddInVersions(Models.User.AddInsModel target, long userId)
        {
            var query = string.Format("users/{0}/addins", userId);
            var response = _gatewayRestService.Get("Security", "latest", query);

            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
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
        }

        private void PopulateApplicationVersions(Models.User.AddInsModel target, long userId)
        {
            var query = string.Format("users/{0}/applications", userId);
            var response = _gatewayRestService.Get("Security", "latest", query);

            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
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
        }

        public string[] DeleteUserAddInVersions(long userId, long addInVersionId)
        {
            var query = string.Format("users/{0}/addins/{1}", userId, addInVersionId);
            var response = _gatewayRestService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public string[] DeleteUserApplicationVersions(long userId, long applicationVersionId)
        {
            var query = string.Format("users/{0}/applications/{1}", userId, applicationVersionId);
            var response = _gatewayRestService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public string[] InsertUserAddInVersions(long groupId, AddInVersionModel addInVersion)
        {
            var query = string.Format("users/{0}/addins", groupId);
            var response = _gatewayRestService.Put("Security", "latest", query, addInVersion.Serialize());

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public string[] InsertUserApplicationVersions(long groupId, ApplicationVersionModel version)
        {
            var query = string.Format("users/{0}/applications", groupId);
            var response = _gatewayRestService.Put("Security", "latest", query, version.Serialize());

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Content.Message };
        }

        public IEnumerable<BusinessFunction> GetBusinessFunctions()
        {
            var query = "businessfunctions";
            var response = _gatewayRestService.Get("security", "latest", query);

            if (!response.Successfull)
                return null;

            var element = response.Content.GetPayloadAsXElement();

            return element.Descendants("BusinessFunction")
                .Select(item => item.Deserialize<BusinessFunction>())
                .ToList();
        }

        public string[] Create(BusinessFunction model)
        {
            var query = "businessfunctions";
            var response = _gatewayRestService.Post("security", "latest", query, model.Serialize());

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public string[] DeleteBusinessFunction(int id)
        {
            var query = string.Format("businessfunctions/{0}", id);
            var response = _gatewayRestService.Delete("security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public IEnumerable<GroupType> GetGroupTypes()
        {
            var query = "grouptypes";
            var response = _gatewayRestService.Get("security", "latest", query);

            if (!response.Successfull)
                return null;

            var element = response.Content.GetPayloadAsXElement();

            return element.Descendants("GroupType")
                .Select(item => item.Deserialize<GroupType>())
                .ToList();
        }

        public string[] Create(GroupType model)
        {
            var query = "grouptypes";
            var response = _gatewayRestService.Post("security", "latest", query, model.Serialize());

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public string[] DeleteGroupType(int id)
        {
            var query = string.Format("grouptypes/{0}", id);
            var response = _gatewayRestService.Delete("security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public bool GenerateDocumentation(string id, string version)
        {
            var query = string.Format("controllers/{0}/versions/{1}/documentation", id, version);
            var response = _gatewayRestService.Put("Catalogue", "latest", query, string.Empty);
            return response.Successfull;
        }

        public string[] UpdateGroupBusinessFunction(string groupId, string businessFunctionId)
        {
            var query = string.Format("groups/{0}/businessfunction/{1}", groupId, businessFunctionId);
            var response = _gatewayRestService.Put("security", "latest", query, null);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        private string GetDefaultKnownGateways(string environment)
        {
            var env = (environment ?? string.Empty).ToUpper();
            switch (env)
            {
                case "DEV":
                    return "jhbdsm020000245;jhbdsm020000244";
                case "UAT":
                    return "jhbpsm020000757;jhbpsm020000758";
                case "PRD":
                case "PROD":
                default:
                    return "JHBPSM050000114";
            }
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

        private ServerResponse Fetch(string query, params string[] args)
        {
            var url = args != null && args.Length > 0 ? string.Format(query, args) : query;

            foreach (var gateway in _gateways)
            {
                var document = Get(gateway, url);
                return new ServerResponse(gateway, document);
            }
            return null;
        }

        private IEnumerable<ServerResponse> FetchAll(string query, params string[] args)
        {
            var url = args != null && args.Length > 0 ? string.Format(query, args) : query;
            //TODO: Update gateway endpoints to return results for all gateways then remove this method.
            foreach (var gateway in _gateways)
            {
                var document = Get(gateway, url);
                if (document != null)
                    yield return new ServerResponse(gateway, document);
            }
        }

        private XDocument Get(string gateway, string query)
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
                var response = client.GetAsync(uri);
                response.Wait(_defaultRequestTimeout);

                if (response.Result.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                var responseContent = response.Result.Content.ReadAsStringAsync();
                responseContent.Wait();

                return XDocument.Parse(responseContent.Result);
            }
        }

        private async Task Delete(string query, params string[] args)
        {
            query = args != null && args.Length > 0 ? string.Format(query, args) : query;

            foreach (var gateway in _gateways)
            {
                var url = string.Format("http://{0}:{1}/{2}", gateway, _port, query);

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
        }

        private void PopulateAvailableSystems(PermissionsModel target)
        {
            var response = _gatewayRestService.Get("Security", "latest", "systems");

            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<SelectListItem>();
                foreach (var item in element.Descendants("SystemName"))
                {
                    var model = item.Deserialize<SystemNameModel>();
                    var foo = new SelectListItem { Text = model.Name, Value = model.Id.ToString() };
                    interim.Add(foo);
                }

                target.AvailableSystems.AddRange(interim.OrderBy(v => v.Text));
            }
        }

        private void PopulateAvailablePermissions(Models.Group.PermissionsModel target)
        {
            var response = _gatewayRestService.Get("Security", "latest", "permissions");

            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<SelectListItem>();
                foreach (var item in element.Descendants("Permission"))
                {
                    var model = item.Deserialize<PermissionModel>();
                    var foo = new SelectListItem
                    {
                        Text = string.Format("{0} - {1}", model.SystemName, model.Name),
                        Value = model.Id.ToString()
                    };
                    interim.Add(foo);
                }

                target.AvailablePermissions.AddRange(interim.OrderBy(v => v.Text));
            }
        }

        private void PopulateAvailableSites(SitesModel target)
        {
            var response = _gatewayRestService.Get("Security", "latest", "sites");

            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<SelectListItem>();
                foreach (var item in element.Descendants("Site"))
                {
                    var model = item.Deserialize<SiteModel>();
                    var foo = new SelectListItem { Text = model.Name, Value = model.Id.ToString() };
                    interim.Add(foo);
                }

                target.AvailableSites.AddRange(interim.OrderBy(v => v.Text));
            }
        }
        
        private IEnumerable<SelectListItem> GetAvailableApplicationVersions()
        {
            var response = _gatewayRestService.Get("Security", "latest", "applications/versions");

            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                foreach (var model in DeserializeApplicationVersionItems(element).OrderByDescending(v => v.ActualVersion))
                {
                    yield return new SelectListItem
                    {
                        Text = string.Format("{0} - {1}", model.Application, model.Version),
                        Value = string.Format("{0}|{1}|Application", model.Application, model.Version)
                    };
                }
            }
        }

        private IEnumerable<SelectListItem> GetAvailableReferencedApplicationVersions()
        {
            var response = _gatewayRestService.Get("Security", "latest", "applications/versions/referenced");

            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                foreach (var model in DeserializeApplicationVersionItems(element).OrderByDescending(v => v.ActualVersion))
                {
                    yield return new SelectListItem
                    {
                        Text = string.Format("{0} - {1}", model.Application, model.Version),
                        Value = string.Format("{0}|{1}|Application", model.Application, model.Version)
                    };
                }
            }
        }

        private IEnumerable<SelectListItem> GetAvailableAddInVersions()
        {
            var response = _gatewayRestService.Get("Security", "latest", "addins/versions");

            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                foreach (var model in DeserializeVersionItems(element).OrderByDescending(v => v.ActualVersion))
                {
                    yield return new SelectListItem
                    {
                        Text = string.Format("{0} - {1}", model.AddIn, model.Version),
                        Value = string.Format("{0}|{1}|Add-In", model.AddIn, model.Version)
                    };
                }
            }
        }

        private IEnumerable<SelectListItem> GetAvailableReferencedAddInVersions()
        {
            var response = _gatewayRestService.Get("Security", "latest", "addins/versions/referenced");

            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                foreach (var model in DeserializeVersionItems(element).OrderByDescending(v => v.ActualVersion))
                {
                    yield return new SelectListItem
                    {
                        Text = string.Format("{0} - {1}", model.AddIn, model.Version),
                        Value = string.Format("{0}|{1}|Add-In", model.AddIn, model.Version)
                    };
                }
            }
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

        private GatewayInfo GetGatewayInfo()
        {
            var serverResponse = Fetch("health/info");

            if (serverResponse == null || 
                serverResponse.Document == null ||
                !serverResponse.Document.Descendants().Any())
                return new GatewayInfo();

            var xmlElement = serverResponse.Document.Descendants().First();
            return xmlElement.Deserialize<GatewayInfo>();
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
    }
}