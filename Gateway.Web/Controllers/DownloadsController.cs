using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.Contracts.Utils;
using Gateway.Web.Database;
using Gateway.Web.Services;
using Controller = System.Web.Mvc.Controller;
using Version = System.Version;

namespace Gateway.Web.Controllers
{
    [RoutePrefix("downloads")]
    public class DownloadsController : BaseController
    {
        private string RemoteAppsDirectory { get; }
        public const string RemoteAppsDirectoryKey = "RemoteAppsDirectory";
        private readonly IGatewayDatabaseService _database;
        private readonly IDifferentialDownloadService _differentialDownloadService;
        private readonly ILogger _logger;

        public DownloadsController(
            IDifferentialDownloadService differentialDownloadService,
            IGatewayDatabaseService database,
            ILoggingService loggingService,
            ISystemInformation information)
            : base(loggingService)
        {
            _differentialDownloadService = differentialDownloadService;
            _database = database;
            string remoteAppsDirectory;
            if (!information.TryGetSetting(RemoteAppsDirectoryKey, out remoteAppsDirectory))
            {
                throw new ConfigurationErrorsException("Missing configuration key: " + RemoteAppsDirectoryKey);
            }
            RemoteAppsDirectory = remoteAppsDirectory;
            _logger = loggingService.GetLogger(this);
        }



        [HttpGet]
        [Route("latest/{app}")]
        [AllowAnonymous]
        public ActionResult GetLatest(string app)
        {
            _logger.InfoFormat("A user located at {0} is trying to determine the latest version of {1}", Request.UserHostAddress, app);

            // Check that the passed parameters conform to expectations to prevent injection attacks.
            if (!IsValidAppParameter(app))
                throw new InvalidOperationException("Invalid parameter (app): " + app);

            var latest = GetLatestRemoteAppVersion(app);
            var content = latest != null ? latest.ToString() : string.Empty;

            var contentType = System.Net.Mime.MediaTypeNames.Text.Plain;
            return Content(content, contentType);
        }

        [HttpGet]
        [Route("{app}/{version}")]
        [AllowAnonymous]
        [OutputCache(Duration = 600, Location = OutputCacheLocation.ServerAndClient, VaryByParam = "app;version")]    // Cache for ten minutes
        public ActionResult Downloads(string app, string version)
        {
            _logger.InfoFormat("A user located at {0} is attempting to download {1} version {2}", Request.UserHostAddress, app, version);

            // Check that the passed parameters conform to expectations to prevent injection attacks.
            if (!IsValidAppParameter(app))
                throw new InvalidOperationException("Invalid parameter (app): " + app);

            // Allow for resolution of latest
            if (string.Equals(version, "latest", StringComparison.CurrentCultureIgnoreCase))
            {
                var latest = GetLatestRemoteAppVersion(app);
                version = latest != null ? latest.ToString() : "latest";
            }

            if (!IsValidVersionParameter(version))
                throw new InvalidOperationException("Invalid parameter (version): " + version);

            string path, fileName;
            if (!GetRemoteAppVersionPath(app, version, out path, out fileName))
                return HttpNotFound(string.Format("Could not find application {0} with version {1}", app, version));

            var contentType = System.Net.Mime.MediaTypeNames.Application.Octet;
            return File(path, contentType, fileName);
        }

        [HttpGet]
        [Route("upgrade/{app}/from/{from}/to/{to}")]
        [AllowAnonymous]
        public ActionResult DownloadDiff(string app, string from, string to)
        {
            _logger.InfoFormat("A user located at {0} is attempting to download a diff of {1} from version {2} to version {3}", Request.UserHostAddress, app, from, to);

            // Check that the passed parameters conform to expectations to prevent injection attacks.
            if (!IsValidAppParameter(app))
                throw new InvalidOperationException("Invalid parameter (app): " + app);
            if (!IsValidVersionParameter(from))
                throw new InvalidOperationException("Invalid parameter (version): " + from);
            if (!IsValidVersionParameter(to))
                throw new InvalidOperationException("Invalid parameter (version): " + to);

            var differential = _differentialDownloadService.Get(app, from, to);
            if (!differential.IsValid)
                return HttpNotFound(string.Format("Could not find upgrade of {0} from version {1} to {2}", app, from, to));

            var contentType = System.Net.Mime.MediaTypeNames.Application.Octet;
            return File(differential.FullName, contentType, differential.Name);
        }

        [HttpGet]
        [Route("links")]
        [AllowAnonymous]
        public ActionResult GetLinks()
        {
            _logger.InfoFormat("A user located at {0} is trying fetch links", Request.UserHostAddress);

            var links = _database.GetLinks();
            var content = links.Serialize();

            var contentType = System.Net.Mime.MediaTypeNames.Text.Plain;
            return Content(content, contentType);
        }

        private Version GetLatestRemoteAppVersion(string app)
        {
            var versions = GetRemoteAppVersions(app);
            return versions.OrderBy(v => v).LastOrDefault();
        }

        private IEnumerable<Version> GetRemoteAppVersions(string app)
        {
            // Determine if path is valid.
            var path = string.Format("{0}\\{1}\\", RemoteAppsDirectory, app);
            var directories = Directory.GetDirectories(path);
            var versions = new List<Version>();

            foreach (var directory in directories)
            {
                Version version;
                var info = new DirectoryInfo(directory);
                if (Version.TryParse(info.Name, out version))
                    versions.Add(version);
            }

            return versions;
        }

        private bool GetRemoteAppVersionPath(string app, string version, out string path, out string fileName)
        {
            // Determine if path is valid.
            path = string.Format("{0}\\{1}\\{2}\\", RemoteAppsDirectory, app, version);
            fileName = "";
            if (!Directory.Exists(path))
                return false;

            // Find file to download.
            var files = Directory.GetFiles(path);
            if (files.Length <= 0) return false;

            // If there is more than one file in the target folder then take the largest?
            if (files.Length > 1)
            {
                path = files.Select(f => new FileInfo(f))
                    .OrderBy(f => f.Length)
                    .Select(f => f.FullName)
                    .LastOrDefault();
            }
            else
            {
                path = files[0];
            }

            fileName = Path.GetFileName(path);
            return true;
        }

        private bool IsValidVersionParameter(string version)
        {
            // Check that version string is valid SEMVAR
            System.Version v;
            return System.Version.TryParse(version, out v);
        }

        private bool IsValidAppParameter(string app)
        {
            // Check that app name is only alpha numeric.
            return app.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c.Equals('-'));
        }
    }
}