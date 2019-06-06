using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MIT.Redis;
using Gateway.Web.Database;
using Gateway.Web.Models.ServerResource;
using Controller = System.Web.Mvc.Controller;
using Gateway.Web.Authorization;

namespace Gateway.Web.Controllers
{
    [RoutePrefix("ServerResource")]
    public class ServerResourceController : BaseController
    {
        private readonly IGatewayDatabaseService _databaseService;

        public ServerResourceController(IGatewayDatabaseService databaseService,
            ILoggingService loggingService)
            :base(loggingService)
        {
            _databaseService = databaseService;
        }

        public ActionResult Index()
        {
            var serverResourceModel = new ServerResourceModel
            {
                Servers = _databaseService.GetServers().ToList()
            };

            return View("Index", serverResourceModel);
        }

        public ActionResult ServerControllers(string id)
        {
            int serverid;

            if (!Int32.TryParse(id, out serverid))
            {
                return RedirectToAction("Index");
            }

            var model = _databaseService.GetSeverControllers(serverid);

            return View("ServerControllers", model);

        }

        [HttpPost]
        [RoleBasedAuthorize(Roles = "Security.Modify")]
        public ActionResult SaveServerControllers(ServerControllerModel model)
        {
            try
            {
                _databaseService.UpdateServerControllers(model);
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View("ServerControllers", model);
        }

    }


}