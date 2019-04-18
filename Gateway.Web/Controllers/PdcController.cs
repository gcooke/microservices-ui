using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;
using Gateway.Web.Models.Pdc;
using Gateway.Web.Services.Pdc;
using System.Web.Mvc;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.Modify")]
    public class PdcController : BaseController
    {
        private readonly IPdcService _pdcService;

        public PdcController(ILoggingService loggingService, IPdcService pdcService) : base(loggingService)
        {
            _pdcService = pdcService;
        }

        public ActionResult Index()
        {
            var model = Load();
            return View("Services", model);
        }

        [HttpPost]
        public ActionResult Ping(PdcServicesModel model)
        {
            model = Load();
            var results = _pdcService.PingAll(model.Items); 
            return View("Services", model);
        }

        private PdcServicesModel Load()
        {
            var model = new PdcServicesModel();
            var instances = _pdcService.GetInstances();
            model.Load(instances);
            return model;
        }
    }
}