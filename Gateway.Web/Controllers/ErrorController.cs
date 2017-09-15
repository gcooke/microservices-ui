using System.Threading;
using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;

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
            _logger.ErrorFormat("User {0} does not have access.", Thread.CurrentPrincipal.Identity.Name);
            return View("Unauthorized");
        }
    }
}