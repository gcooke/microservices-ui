﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gateway.Web.Controllers
{
    public class ErrorController : Controller
    {
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
    }
}