using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.Contracts.Utils;

namespace Gateway.Web.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger _logger;
        public ErrorController(ILoggingService loggingService)
        {
            _logger = loggingService.GetLogger(this);
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View("Error");
        }

        [AllowAnonymous]
        public ActionResult NotFound()
        {
            Response.StatusCode = 200;
            return View("NotFound");
        }

        [AllowAnonymous]
        public ActionResult InternalServer()
        {
            Response.StatusCode = 200;
            return View("InternalServer");
        }

        [AllowAnonymous]
        public ActionResult Unauthorized()
        {
            Response.StatusCode = 200;
            return View("Unauthorized");
        }
    }
}