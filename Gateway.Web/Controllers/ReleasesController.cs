using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;

namespace Gateway.Web.Controllers
{
    public class ReleasesController : BaseController
    {
        public ReleasesController(ILoggingService loggingService) : base(loggingService)
        {
        }

        public ActionResult Current()
        {
            return View("Current");
        }

        public ActionResult Historical()
        {
            return View("Historical");
        }
    }
}