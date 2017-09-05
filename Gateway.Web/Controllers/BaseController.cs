using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;

namespace Gateway.Web.Controllers
{
    public class BaseController : Controller
    {
        private ILogger _logger;

        public BaseController(ILoggingService loggingService)
        {
            _logger = loggingService.GetLogger(this);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;
            var url = filterContext.RequestContext.HttpContext.Request.RawUrl;

            _logger.ErrorFormat("[{0}] accessing {1}: {2}/n {3}",
                System.Threading.Thread.CurrentPrincipal.Identity.Name,
                url,
                filterContext.Exception.Message,
                filterContext.Exception.StackTrace);

            // Redirect on error:
            filterContext.Result = RedirectToAction("Index", "Error");
        }
    }
}