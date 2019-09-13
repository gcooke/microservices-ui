using Bagl.Cib.MIT.Logging;
using Gateway.Web.Database;
using Gateway.Web.Models.ServerResource;
using Gateway.Web.Services;
using System;
using System.Linq;
using System.Web.Mvc;
using Gateway.Web.Authorization;

namespace Gateway.Web.Controllers
{
    [RoutePrefix("ServerResource")]
    public class ServerResourceController : BaseController
    {
        private readonly IGatewayDatabaseService _databaseService;
        private readonly IGatewayService _gatewayService;

        public ServerResourceController(
            IGatewayDatabaseService databaseService,
            IGatewayService gatewayService,
            ILoggingService loggingService
            ) : base(loggingService)
        {
            _databaseService = databaseService;
            _gatewayService = gatewayService;
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

                _logger.Info($"_databaseService.UpdateServerController called Successfully.");

                try
                {
                    _gatewayService.NotifyResourceUpdate();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Server Resource Notification to Gateway has failed.");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                _logger.Error(ex, "Error saving Server Controllers.");
            }

            return View("ServerControllers", model);
        }
    }
}