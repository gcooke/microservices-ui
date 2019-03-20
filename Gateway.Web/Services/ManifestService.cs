using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            result.Tables.Add(ExtractServerTable(manifest));

            // Extract controller versions
            result.Tables.Add(ExtractControllerTable(manifest));

            return result;
        }

        private ReportTable ExtractServerTable(Manifest manifest)
        {
            // Determine list of applicable servers
            var applicable = new List<Server>();
            foreach (var item in manifest.Servers)
                if (string.Equals(item.Environment, _information.EnvironmentName, StringComparison.CurrentCultureIgnoreCase))
                    applicable.Add(item);

            var result = new ReportTable();
            result.Title = "Assigned Services";
            result.Columns.Add("Server");
            result.Columns.Add("Type");
            result.Columns.Add("Item");
            result.Columns.Add("Version");

            foreach (var server in applicable.OrderBy(a => a.Name))
            {
                var name = server.Name;
                foreach (var website in server.Websites)
                {
                    var version = manifest.Software.Websites.FirstOrDefault(w => string.Equals(w.Name, website.Name, StringComparison.CurrentCultureIgnoreCase)) ??
                                  manifest.Software.SinglePageApplication.FirstOrDefault(w => string.Equals(w.Name, website.Name, StringComparison.CurrentCultureIgnoreCase));

                    var row = new ReportRows();
                    row.Values.Add(name);
                    row.Values.Add("Website");
                    row.Values.Add(website.Name);
                    row.Values.Add(version?.Version);
                    result.Rows.Add(row);

                    name = string.Empty;
                }

                foreach (var service in server.WindowsServices)
                {
                    var version = manifest.Software.WindowsServices.FirstOrDefault(w => string.Equals(w.Name, service.Name, StringComparison.CurrentCultureIgnoreCase));

                    var row = new ReportRows();
                    row.Values.Add(name);
                    row.Values.Add("Service");
                    row.Values.Add(service.Name);
                    row.Values.Add(version?.Version);
                    result.Rows.Add(row);

                    name = string.Empty;
                }
            }

            return result;
        }

        private ReportTable ExtractControllerTable(Manifest manifest)
        {
            var result = new ReportTable();
            result.Title = "Controller Versions";
            result.Columns.Add("Controller");
            result.Columns.Add("Alias");
            result.Columns.Add("Version");

            foreach (var controller in manifest.Controllers.OrderBy(a => a.Name))
            {
                var name = controller.Name;
                foreach (var version in controller.versions)
                {
                    var row = new ReportRows();
                    row.Values.Add(name);
                    row.Values.Add(version.Alias);
                    row.Values.Add(version.Name);
                    result.Rows.Add(row);

                    name = string.Empty;
                }
            }

            return result;
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