using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;

namespace Gateway.Web.Controllers
{
    /// <summary>
    /// Risk Batch Harness endpoint
    /// </summary>
    [RoutePrefix("rbh")]
    public class RbhController : BaseController
    {
        private readonly ILogger _logger;
        private readonly string[] _packageLocations;

        public RbhController(ILoggingService loggingService,
            ISystemInformation systemInformation)
            : base(loggingService)
        {
            _logger = loggingService.GetLogger(this);
            _packageLocations = systemInformation.GetSetting("Pnr.PricingLocation")?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private bool IsValidVersionParameter(string version)
            => version.All(c => char.IsDigit(c) || c == '.');

        private ActionResult GetPnrVersionImpl(string version)
        {
            if (_packageLocations?.Length == null)
                throw new Exception("Pnr.PricingLocation has not been configured.");

            if (string.IsNullOrEmpty(version))
                throw new ArgumentException("Unexpected null or empty string.", nameof(version));

            version = version.Replace("-", ".");
            _logger.InfoFormat("Version of PnR (pricing) requested '{0}'.", version);

            if (!IsValidVersionParameter(version))
                throw new Exception($"'{version}' is not an valid version representation.");

            foreach (var location in _packageLocations)
            {
                var sourcePath = Path.Combine(location, version);

                if (!Directory.Exists(sourcePath))
                    continue;

                var sourceZipFile = Directory.GetFiles(sourcePath, "*.zip")
                    .FirstOrDefault() 
                    ?? throw new Exception($"Could not find a pricing controller zip for '{version}'.");

                _logger.InfoFormat("Returning '{0}'.", sourceZipFile);
                return File(sourceZipFile, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(sourceZipFile));
            }

            return new HttpNotFoundResult($"Version '{version}' cannot be found.");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("pnr/{version}")]
        public ActionResult GetPnrVersion(string version)
            => GetPnrVersionImpl(version);
    }
}