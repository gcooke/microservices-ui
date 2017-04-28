﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Utils;

namespace Gateway.Web.Services
{
    public class GatewayService : IGatewayService
    {
        private readonly string[] _gateways = new[] { "JHBPSM020000757", "JHBPSM020000758" };
        private readonly int _port = 7010;

        public GatewayService()
        {

        }

        public ServersModel GetServers()
        {
            var result = new ServersModel();
            foreach (var gateway in _gateways)
                result.Servers.Add(new Server() { Name = gateway });
            return result;
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

        public XElement GetReport(string report)
        {
            var doc = Fetch("api/reporting/latest/{0}", report).FirstOrDefault();

            var element = doc.Document.Descendants("RiskValue").FirstOrDefault();
            return element;
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
                            var result = new QueueModel()
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

        private IEnumerable<ServerResponse> Fetch(string format, params string[] args)
        {
            foreach (var gateway in _gateways)
            {
                var url = (args != null && args.Length > 0) ? string.Format(format, args) : format;
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
            catch (Exception ex)
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

            public string Server { get; set; }
            public XDocument Document { get; set; }
        }
    }
}