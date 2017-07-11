using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Database;
using Gateway.Web.Models.AddIn;
using Gateway.Web.Models.Security;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    [RoutePrefix("downloads")]
    public class DownloadsController : Controller
    {
        private const string AppsDirectory = @"\\Intranet.barcapint.com\dfs-emea\Group\Jhb\IT_Pricing_Risk\Builds\Redstone\Apps";

        private readonly IGatewayService _gateway;
        private readonly ILogger _logger;

        public DownloadsController(IGatewayService gateway, ILoggingService loggingService)
        {
            _gateway = gateway;
            _logger = loggingService.GetLogger(this);
        }

        [HttpGet]
        [Route("{app}/{version}")]
        public ActionResult Downloads(string app, string version)
        {
            _logger.InfoFormat("A user located at {0} is attempting to download {1} version {2}", Request.UserHostAddress, app, version);

            // Check that the passed parameters conform to expectations to prevent injection attacks.
            if (!IsValidAppParameter(app))
                throw new InvalidOperationException("Invalid parameter (app): " + app);
            if (!IsValidVersionParameter(version))
                throw new InvalidOperationException("Invalid parameter (version): " + version);

            string path, fileName;
            if (!GetAppVersionPath(app, version, out path, out fileName))
                return HttpNotFound(string.Format("Could not find application {0} with version {1}", app, version));

            var contentType = string.Format("application/{0}", Path.GetExtension(fileName)).ToLower();
            return File(path, contentType, fileName);
        }

        private bool GetAppVersionPath(string app, string version, out string path, out string fileName)
        {
            // Determine if path is valid.
            path = string.Format("{0}\\{1}\\{2}\\", AppsDirectory, app, version);
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
            return app.All(char.IsLetterOrDigit);
        }
    }
}