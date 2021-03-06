﻿using System.Web.Mvc;
using System.Web.Routing;
using Bagl.Cib.MIT.Logging;

namespace Gateway.Web.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ILogger _logger;

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
            var result = this.View("~/Views/Error/Error.cshtml", new HandleErrorInfo(filterContext.Exception,
                filterContext.RouteData.Values["controller"].ToString(),
                filterContext.RouteData.Values["action"].ToString()));
            filterContext.Result = result;
        }
    }
}