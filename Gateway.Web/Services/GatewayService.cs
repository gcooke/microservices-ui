using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Models.Request;
using Gateway.Web.Utils;
using Newtonsoft.Json;
using Version = Gateway.Web.Models.Controller.Version;

namespace Gateway.Web.Services
{
    public class GatewayService : IGatewayService
    {
        private readonly string[] _gateways;
        private readonly ILogger _logger;
        private readonly int _port = 7010;
        private readonly IGatewayRestService _restService;

        public GatewayService(ISystemInformation information, IGatewayRestService restService,
            ILoggingService loggingService)
        {
            _restService = restService;
            _logger = loggingService.GetLogger(this);
            var gateways = information.GetSetting("KnownGateways", GetDefaultKnownGateways(information.EnvironmentName));
            _gateways = gateways.Split(';');
        }

        public ServersModel GetServers()
        {
            if (!_gateways.Any())
                return null;

            var gateway = _gateways.First();
            var url = string.Format("http://{0}:{1}/{2}", gateway, _port, "health/info");
            var document = Fetch(url);

            if (document == null || !document.Descendants().Any())
                return null;

            var xmlElement = document.Descendants().First();
            var servers = xmlElement.Deserialize<ArrayOfGatewayInfo>();
            return new ServersModel(servers);
        }

        public WorkersModel GetWorkers()
        {
            var docs = Fetch("health/services", string.Empty);
            var result = GetWorkers("All", docs);

            // Populate process information
            docs = Fetch("health/processes/{0}", "Bagl.Cib.MSF.ControllerHost");
            return PopulateProcessInfo(result, docs);
        }

        public WorkersModel GetWorkers(string controller)
        {
            var docs = Fetch("health/services/{0}", controller);
            return GetWorkers(controller, docs);
        }

        public IEnumerable<QueueModel> GetCurrentQueues(string controller)
        {
            var docs = Fetch("health/queues/{0}", controller);
            return GetCurrentQueues(docs);
        }

        public IEnumerable<QueueModel> GetCurrentQueues()
        {
            var docs = Fetch("health/queues", string.Empty);
            return GetCurrentQueues(docs);
        }

        public XElement[] GetReport(string report)
        {
            var doc = Fetch("api/riskdata/latest/{0}", report).FirstOrDefault();

            var element = doc.Document.Descendants("xVAReturnResult").ToArray();
            return element;
        }

        public string[] GetSites()
        {
            var doc = Fetch("api/tradestore/latest/{0}", "LegalEntities").FirstOrDefault();

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
                Fetch("api/Catalogue/latest/tree/{0}?includepayloads=false", correlationId.ToString()).FirstOrDefault();
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
            var response = Fetch("api/Catalogue/latest/controllers/{0}", name).FirstOrDefault();
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

        private IEnumerable<ServerResponse> FetchTest(string url)
        {
            foreach (var gateway in _gateways)
            {
                //var url = (args != null && args.Length > 0) ? string.Format(format, args) : format;
                url = string.Format("http://{0}:{1}/{2}", gateway, _port, url);
                var document = Fetch(url);
                if (document != null)
                    yield return new ServerResponse(gateway, document);
            }
        }

        private IEnumerable<ServerResponse> Fetch(string format, params string[] args)
        {
            foreach (var gateway in _gateways)
            {
                var url = args != null && args.Length > 0 ? string.Format(format, args) : format;
                url = string.Format("http://{0}:{1}/{2}", gateway, _port, url);
                var document = Fetch(url);
                if (document != null)
                    yield return new ServerResponse(gateway, document);
            }
        }

        private XDocument Fetch(string uri)
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
                    response.Wait();
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