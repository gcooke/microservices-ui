using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Models.Security;
using Gateway.Web.Utils;

namespace Gateway.Web.Services
{
    public class ManifestService : IManifestService
    {
        private const string RemoteUrl = "https://bitbucket.absa.co.za/rest/api/1.0/projects/CIB_PNR/repos/infrastructure-manifest/raw/windows/SigmaManifest.xml?at=refs/heads/{0}";
        private readonly ISystemInformation _information;
        private readonly ILogger _logger;

        public ManifestService(ILoggingService loggingService, ISystemInformation information)
        {
            _information = information;
            _logger = loggingService.GetLogger(this);
        }

        public ReportsModel GetReport()
        {
            var result = new ReportsModel("Manifest");

            _logger.InfoFormat("Fetching environment manifest from GIT");
            var url = string.Format(RemoteUrl, _information.EnvironmentName.ToLower());
            var listing = GetContent(url);

            var manifest = listing.Deserialize<Manifest>();

            // Extract server information
            result.Add(ExtractServerTable(manifest));

            // Extract controller versions
            result.Add(ExtractControllerTable(manifest));

            return result;
        }

        private ICube ExtractServerTable(Manifest manifest)
        {
            // Determine list of applicable servers
            var applicable = new List<Server>();
            foreach (var item in manifest.Servers)
                if (string.Equals(item.Environment, _information.EnvironmentName, StringComparison.CurrentCultureIgnoreCase))
                    applicable.Add(item);

            var result = new CubeBuilder()
                .AddColumn("Server")
                .AddColumn("Type")
                .AddColumn("Item")
                .AddColumn("Version")
                .Build();
            result.SetAttribute("Title", "Assigned Services");
            
            foreach (var server in applicable.OrderBy(a => a.Name))
            {
                var name = server.Name;
                foreach (var website in server.Websites)
                {
                    var version = manifest.Software.Websites.FirstOrDefault(w => string.Equals(w.Name, website.Name, StringComparison.CurrentCultureIgnoreCase)) ??
                                  manifest.Software.SinglePageApplication.FirstOrDefault(w => string.Equals(w.Name, website.Name, StringComparison.CurrentCultureIgnoreCase));

                    result.AddRow(new object[]
                    {
                        name,
                        "Website",
                        website.Name,
                        version?.Version
                    });

                    name = string.Empty;
                }

                foreach (var service in server.WindowsServices)
                {
                    var version = manifest.Software.WindowsServices.FirstOrDefault(w => string.Equals(w.Name, service.Name, StringComparison.CurrentCultureIgnoreCase));

                    result.AddRow(new object[]
                    {
                        name,
                        "Service",
                        service.Name,
                        version?.Version
                    });
                    name = string.Empty;
                }
            }

            return result;
        }

        private ICube ExtractControllerTable(Manifest manifest)
        {
            var table = new CubeBuilder()
                .AddColumn("Controller")
                .AddColumn("Alias")
                .AddColumn("Version")
                .Build();
            table.SetAttribute("Title", "Controller versions");
            
            foreach (var controller in manifest.Controllers.OrderBy(a => a.Name))
            {
                var name = controller.Name;
                foreach (var version in controller.versions)
                {
                    table.AddRow(new object[]
                    {
                        name,
                        version.Alias,
                        version.Name
                    });

                    name = string.Empty;
                }
            }

            return table;
        }

        private string GetContent(string url)
        {
            HttpWebResponse response = null;

            try
            {
                _logger.Info("Calling " + url);

                var request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.AllowAutoRedirect = true;
                request.UseDefaultCredentials = true;
                request.Method = "GET";

                response = (HttpWebResponse)request.GetResponse();
                var code = response.StatusCode;
                if (code != HttpStatusCode.OK)
                    throw new InvalidOperationException("Cannot access bitbucket: " + code);

                var sr = new StreamReader(response.GetResponseStream());
                return sr.ReadToEnd();
            }
            finally
            {
                response?.Close();
            }
        }
    }
}