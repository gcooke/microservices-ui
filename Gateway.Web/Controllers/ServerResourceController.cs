using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MIT.Redis;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Bagl.Cib.MSF.ClientAPI.Model;
using Bagl.Cib.MSF.ClientAPI.Utils;
using Gateway.Web.Database;
using Gateway.Web.Models.ServerResource;
using Controller = System.Web.Mvc.Controller;
using Gateway.Web.Authorization;
using Bagl.Cib.MSF.ClientAPI.Utils;

namespace Gateway.Web.Controllers
{
    [RoutePrefix("ServerResource")]
    public class ServerResourceController : BaseController
    {
        private readonly IGatewayDatabaseService _databaseService;
        private ISimpleRestService _restService;
        private readonly ISystemInformation _systemInformation;
        private readonly IGateway _gateway;
        private string _baseURL;

        public ServerResourceController(
            IGatewayDatabaseService databaseService,
            ILoggingService loggingService,
            ISimpleRestService restService,
            ISystemInformation systemInformation
            )
            :base(loggingService)
        {
            _databaseService = databaseService;
            _restService = restService;
            _systemInformation = systemInformation;
            _baseURL = $"https://{DefaultKnownGateways.Get(systemInformation)}:7010/";
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
        public async Task<ActionResult> SaveServerControllers(ServerControllerModel model, CancellationToken token)
        {
            try
            {
                _databaseService.UpdateServerControllers(model);

                try
                {
                    var result = await _restService.Get(_baseURL + "resources/notify", token);
                    _logger.Info($"ServerController Resources updated. : Server Response :{result}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Server Resource Notification to Gateway has failed.");
                }

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