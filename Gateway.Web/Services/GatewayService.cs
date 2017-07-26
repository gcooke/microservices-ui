using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Models.AddIn;
using Gateway.Web.Models.Group;
using Gateway.Web.Models.Permission;
using Gateway.Web.Models.Request;
using Gateway.Web.Utils;
using Gateway.Web.Models.Security;
using Gateway.Web.Models.User;
using Newtonsoft.Json;
using AddInsModel = Gateway.Web.Models.Group.AddInsModel;
using PermissionsModel = Gateway.Web.Models.Group.PermissionsModel;
using PortfoliosModel = Gateway.Web.Models.Group.PortfoliosModel;
using SitesModel = Gateway.Web.Models.Group.SitesModel;
using Version = Gateway.Web.Models.Controller.Version;

namespace Gateway.Web.Services
{
    public class GatewayService : IGatewayService
    {
        private readonly string[] _gateways;
        private readonly ILogger _logger;
        private readonly int _port = 7010;
        private readonly IGatewayRestService _restService;
        private readonly TimeSpan _defaultRequestTimeout;

        public GatewayService(ISystemInformation information, IGatewayRestService restService,
            ILoggingService loggingService)
        {
            _defaultRequestTimeout = TimeSpan.FromSeconds(10);
            _restService = restService;
            _logger = loggingService.GetLogger(this);
            var gateways = information.GetSetting("KnownGateways", GetDefaultKnownGateways(information.EnvironmentName));
            _gateways = gateways.Split(';');
        }

        public ServersModel GetServers()
        {
            if (!_gateways.Any())
                return null;

            foreach (var gateway in _gateways)
            {
                try
                {
                    var url = string.Format("http://{0}:{1}/{2}", gateway, _port, "health/info");
                    var document = Fetch(_defaultRequestTimeout, url);

                    if (document == null || !document.Descendants().Any())
                        continue;

                    var xmlElement = document.Descendants().First();
                    var servers = xmlElement.Deserialize<ArrayOfGatewayInfo>();
                    return new ServersModel(servers);
                }
                catch
                {
                }
            }
            return new ServersModel();
        }

        public WorkersModel GetWorkers()
        {
            var docs = Fetch(_defaultRequestTimeout, "health/services", string.Empty);
            var result = GetWorkers("All", docs);

            // Populate process information
            docs = Fetch(_defaultRequestTimeout, "health/processes/{0}", "Bagl.Cib.MSF.ControllerHost");
            return PopulateProcessInfo(result, docs);
        }

        public WorkersModel GetWorkers(string controller)
        {
            var docs = Fetch(_defaultRequestTimeout, "health/services/{0}", controller);
            return GetWorkers(controller, docs);
        }

        public IEnumerable<QueueModel> GetCurrentQueues(string controller)
        {
            var docs = Fetch(_defaultRequestTimeout, "health/queues/{0}", controller);
            return GetCurrentQueues(docs);
        }

        public IEnumerable<QueueModel> GetCurrentQueues()
        {
            var docs = Fetch(_defaultRequestTimeout, "health/queues", string.Empty);
            return GetCurrentQueues(docs);
        }

        public XElement[] GetReport(string report)
        {
            var doc = Fetch(_defaultRequestTimeout, "api/riskdata/latest/{0}", report).FirstOrDefault();

            var element = doc.Document.Descendants("xVAReturnResult").ToArray();
            return element;
        }

        public string[] GetSites()
        {
            var doc = Fetch(_defaultRequestTimeout, "api/tradestore/latest/{0}", "LegalEntities").FirstOrDefault();

            var element = doc.Document.Descendants("Result").ToArray().FirstOrDefault();
            var legalEntities = JsonConvert.DeserializeObject<IEnumerable<string>>(element.Value);

            return legalEntities.ToArray();
        }

        public void ExpireWorkItem(string id)
        {
            //Send to each gateway
            foreach (var gateway in _gateways)
            {
                var url = string.Format("health/queueitem/{0}", id);
                url = string.Format("http://{0}:{1}/{2}", gateway, _port, url);

                var message = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete
                };

                var result = Delete(url, message);
                result.Wait(2000);
            }
        }

        public VersionsModel GetControllerVersions(string name)
        {
            var query = string.Format("controllers/{0}", name);
            var response = _restService.Get("Catalogue", "latest", query);

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
                var response = _restService.Put("Catalogue", "latest", query, content);

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
            var response =
                Fetch(_defaultRequestTimeout, "api/Catalogue/latest/tree/{0}?includepayloads=false", correlationId.ToString()).FirstOrDefault();
            if (response == null)
                return null;

            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("namespace", Constants.RequestPayloadNamespace);
            var xml = response.Document.XPathSelectElement("/namespace:Response/namespace:Payload/Request",
                namespaceManager);

            if (xml == null) return new RequestPayload { ChildRequests = new ChildRequests() };
            return xml.DeserializeUsingDataContract<RequestPayload>();
        }

        public ConfigurationModel GetControllerConfiguration(string name)
        {
            var response = Fetch(_defaultRequestTimeout, "api/Catalogue/latest/controllers/{0}", name).FirstOrDefault();
            if (response == null)
                return null;

            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("namespace", Constants.RequestPayloadNamespace);
            var xml =
                response.Document.XPathSelectElement(
                    "/namespace:Response/namespace:Payload/Catalogue/Controllers/Controller", namespaceManager);

            return xml == null ? null : xml.Deserialize<ConfigurationModel>();
        }

        public RestResponse UpdateControllerConfiguration(ConfigurationModel model)
        {
            var query = "/controllers/configuration";
            return _restService.Put("Catalogue", "latest", query, model.Serialize());
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

        private WorkersModel PopulateProcessInfo(WorkersModel result, IEnumerable<ServerResponse> docs)
        {
            foreach (var doc in docs)
            {
                if (doc.Document == null) continue;

                foreach (var info in doc.Document.Descendants("Process"))
                {
                    var item = info.Deserialize<ProcessInformation>();
                    result.Processes.Add(item);
                }
            }
            return result;
        }

        private WorkersModel GetWorkers(string name, IEnumerable<ServerResponse> docs)
        {
            var result = new WorkersModel(name);
            foreach (var doc in docs)
            {
                if (doc.Document == null) continue;

                foreach (var info in doc.Document.Descendants("ControllerInformation"))
                {
                    var item = info.Deserialize<ControllerInformation>();
                    result.State.Add(item);
                }
            }
            return result;
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

        private IEnumerable<ServerResponse> Fetch(TimeSpan timeout, string format, params string[] args)
        {
            foreach (var gateway in _gateways)
            {
                var url = args != null && args.Length > 0 ? string.Format(format, args) : format;
                url = string.Format("http://{0}:{1}/{2}", gateway, _port, url);
                var document = Fetch(timeout, url);
                if (document != null)
                    yield return new ServerResponse(gateway, document);
            }
        }

        private XDocument Fetch(TimeSpan timeout, string uri)
        {
            try
            {
                using (var client = new HttpClient(new HttpClientHandler
                {
                    UseDefaultCredentials = true,
                    AllowAutoRedirect = true
                }))
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/xml");

                    var response = client.GetAsync(uri);
                    response.Wait(timeout);
                    if (response.Result.StatusCode != HttpStatusCode.OK)
                    {
                        return null;
                    }

                    var responseContent = response.Result.Content.ReadAsStringAsync();
                    responseContent.Wait();

                    return XDocument.Parse(responseContent.Result);
                }
            }
            catch (Exception)
            {
                //TODO: Should somehow output this -fails badly if Gateway is down
                throw;
            }
        }

        private async Task<HttpResponseMessage> Delete(string endpoint, HttpRequestMessage message)
        {
            try
            {
                using (var client = new HttpClient(new HttpClientHandler
                {
                    UseDefaultCredentials = true,
                    AllowAutoRedirect = true
                }))
                {
                    client.Timeout = TimeSpan.FromSeconds(20);
                    var response = await client.DeleteAsync(endpoint);
                    return response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception)
            {
                // Should somehow output this
                return null;
            }
        }

        private async Task<HttpResponseMessage> Post(string endpoint, HttpRequestMessage message)
        {
            try
            {
                using (var client = new HttpClient(new HttpClientHandler
                {
                    UseDefaultCredentials = true,
                    AllowAutoRedirect = true
                }))
                {
                    var response = await client.PostAsync(endpoint, message.Content);
                    return response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception)
            {
                // Should somehow output this
                return null;
            }
        }


        public Models.Security.GroupsModel GetGroups()
        {
            var response = _restService.Get("Security", "latest", "groups");

            var result = new Models.Security.GroupsModel();
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
            var response = _restService.Get("Security", "latest", query);

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

            var result = new Models.Security.ReportsModel();
            result.Name = name;
            return result;
        }

        public ReportsModel GetSecurityReport(string name, string parameter)
        {
            if (!string.IsNullOrEmpty(parameter))
            {
                var query = string.Format("reports/{0}/{1}", name, parameter);
                var response = _restService.Get("Security", "latest", query);

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

            var result = new Models.Security.ReportsModel();
            result.Name = name;
            result.SupportsParameter = true;
            result.Parameter = parameter;
            return result;
        }

        public GroupModel GetGroup(long id)
        {
            var query = string.Format("groups/{0}", id);
            var response = _restService.Get("Security", "latest", query);

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
            var query = string.Format("groups");
            var response = _restService.Put("Security", "latest", query, model.Serialize());

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public string[] DeleteGroup(long id)
        {
            var query = string.Format("groups/{0}", id);
            var response = _restService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public UsersModel GetUsers()
        {
            var response = _restService.Get("Security", "latest", "users");

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

        public UserModel GetUser(string login)
        {
            return new UserModel();
        }

        public Models.Security.AddInsModel GetAddIns()
        {
            var response = _restService.Get("Security", "latest", "addins");

            var result = new Models.Security.AddInsModel();
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<AddInModel>();
                foreach (var item in element.Descendants("AddIn"))
                {
                    interim.Add(item.Deserialize<AddInModel>());
                }
                result.Items.AddRange(interim.OrderBy(v => v.Name));
            }

            return result;
        }

        public AddInModel GetAddIn(long id)
        {
            var query = string.Format("addins/{0}", id);
            var response = _restService.Get("Security", "latest", query);

            var result = new AddInModel();
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                result = element.Deserialize<AddInModel>();
            }

            return result;
        }

        public string[] Create(AddInModel model)
        {
            var query = string.Format("addins");
            var response = _restService.Put("Security", "latest", query, model.Serialize());

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public string[] DeleteAddIn(long id)
        {
            var query = string.Format("addins/{0}", id);
            var response = _restService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }


        public Models.Security.PermissionsModel GetPermissions()
        {
            var response = _restService.Get("Security", "latest", "permissions");

            var result = new Models.Security.PermissionsModel();
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

        private void PopulateAvailableSystems(Models.Security.PermissionsModel target)
        {
            var response = _restService.Get("Security", "latest", "systems");

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

        public PermissionModel GetPermission(long id)
        {
            var query = string.Format("permissions/{0}", id);
            var response = _restService.Get("Security", "latest", query);

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
            var query = string.Format("permissions/{0}", id);
            var response = _restService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public string[] Create(PermissionModel model)
        {
            var query = string.Format("permissions");
            var response = _restService.Put("Security", "latest", query, model.Serialize());

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public string[] InsertGroupPermission(long groupId, long permissionId)
        {
            var query = string.Format("groups/{0}/permissions/{1}", groupId, permissionId);
            var response = _restService.Put("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public ADGroupsModel GetGroupADGroups(long groupId)
        {
            var query = string.Format("groups/{0}/adgroups", groupId);
            var response = _restService.Get("Security", "latest", query);

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
            var response = _restService.Delete("Security", "latest", query, string.Empty);

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
            var response = _restService.Put("Security", "latest", query, payload);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public PermissionsModel GetGroupPermisions(long groupId)
        {
            var query = string.Format("groups/{0}/permissions", groupId);
            var response = _restService.Get("Security", "latest", query);

            var result = new PermissionsModel(groupId);
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

        private void PopulateAvailablePermissions(PermissionsModel target)
        {
            var response = _restService.Get("Security", "latest", "permissions");

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

        public PortfoliosModel GetGroupPortfolios(long groupId)
        {
            var query = string.Format("groups/{0}/portfolios", groupId);
            var response = _restService.Get("Security", "latest", query);

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
            var response = _restService.Get("Security", "latest", query);

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

        public string[] DeleteGroupSite(long id, long groupId)
        {
            var query = string.Format("groups/{0}/sites/{1}", groupId, id);
            var response = _restService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public string[] InsertGroupSite(long groupId, long siteId)
        {
            var query = string.Format("groups/{0}/sites/{1}", groupId, siteId);
            var response = _restService.Put("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        private void PopulateAvailableSites(SitesModel target)
        {
            var response = _restService.Get("Security", "latest", "sites");

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

        public AddInsModel GetGroupAddIns(long groupId)
        {
            var query = string.Format("groups/{0}/addins", groupId);
            var response = _restService.Get("Security", "latest", query);

            var result = new AddInsModel(groupId);
            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<GroupAddInVersionModel>();
                foreach (var item in element.Descendants("GroupAddInVersion"))
                {
                    interim.Add(item.Deserialize<GroupAddInVersionModel>());
                }
                result.Items.AddRange(interim.OrderBy(v => v.Name));
            }

            PopulateAvailableAddIns(result);
            PopulateAvailableVersions(result);
            return result;
        }

        private void PopulateAvailableAddIns(Models.Group.AddInsModel target)
        {
            var response = _restService.Get("Security", "latest", "addins");

            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                foreach (var item in element.Descendants("AddIn"))
                {
                    var model = item.Deserialize<AddInModel>();
                    target.AvailableAddIns.Add(model);
                }
            }
        }

        private void PopulateAvailableVersions(Models.Group.AddInsModel target)
        {
            var response = _restService.Get("Security", "latest", "addins/versions");

            if (response.Successfull)
            {
                var element = response.Content.GetPayloadAsXElement();
                var interim = new List<SelectListItem>();
                foreach (var item in element.Descendants("AddInVersion"))
                {
                    var model = item.Deserialize<AddInVersionModel>();
                    if (string.Equals(model.AddIn, "ExcelTools", StringComparison.CurrentCultureIgnoreCase)) continue;

                    var foo = new SelectListItem
                    {
                        Text = string.Format("{0} - {1}", model.AddIn, model.Version),
                        Value = string.Format("{0}|{1}", model.AddIn, model.Version)
                    };
                    interim.Add(foo);
                }

                target.AvailableVersions.AddRange(interim.OrderBy(v => v.Text));
            }
        }

        public string[] InsertGroupAddInVersion(long groupId, AddInVersionModel addInVersion)
        {
            var query = string.Format("groups/{0}/addins", groupId);
            var response = _restService.Put("Security", "latest", query, addInVersion.Serialize());

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
        }

        public string[] DeleteGroupAddInVersion(long id, long groupId)
        {
            var query = string.Format("groups/{0}/addins/{1}", groupId, id);
            var response = _restService.Delete("Security", "latest", query, string.Empty);

            if (response.Successfull)
            {
                return null;
            }

            return new[] { response.Message };
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
