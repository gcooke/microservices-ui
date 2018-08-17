using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bagl.Cib.MIT.Redis;
using Gateway.Web.Database;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    [RoutePrefix("ServerResource")]
    public class ServerResourceController : Controller
    {
        private readonly IGatewayDatabaseService _databaseService;

        public ServerResourceController(IGatewayDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public ActionResult Index()
        {
            return RedirectToAction("ServerConfiguration");
        }

        public ActionResult ServerConfiguration()
        {
            return View("Index",_databaseService.GetConfiguredServers());
        }

        [Route("DeleteServer/{serverName}/{resourceName}")]
        public ActionResult DeleteServer(string serverName, string resourceName)
        {
            _databaseService.DeleteServerResourceLink(serverName, resourceName);

            return RedirectToAction("ServerConfiguration");
        }

        [Route("AddServer/{serverName}/{resourceName}")]
        public ActionResult AddServer(string serverName, string resourceName)
        {
            _databaseService.AddServerResourceLink(serverName, resourceName);

            return RedirectToAction("ServerConfiguration");
        }



        public ActionResult ControllerConfiguration()
        {
            return View("Index", _databaseService.GetControllerResources());
        }
      

        [Route("DeleteController/{controllerName}/{resourceName}")]
        public ActionResult DeleteController(string controllerName, string resourceName)
        {
            _databaseService.DeleteControllerResourceLink(controllerName, resourceName);

            return RedirectToAction("ControllerConfiguration");
        }

        [Route("AddController/{controllerName}/{resourceName}")]
        public ActionResult AddController(string controllerName, string resourceName)
        {
            _databaseService.AddControllerResourceLink(controllerName, resourceName);

            return RedirectToAction("ControllerConfiguration");
        }
    }


}