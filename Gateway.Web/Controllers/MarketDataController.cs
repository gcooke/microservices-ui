
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Models.MarketData;
using Gateway.Web.Services;

namespace Gateway.Web.Controllers
{
    public class MarketDataController : BaseController
    {
        private readonly IGatewayService _gateway;

        public MarketDataController(IGatewayService gateway, ILoggingService loggingService)
            : base(loggingService)
        {
            _gateway = gateway;

        }

        public async Task<ActionResult> MissingMonikersAction(DateTime? rundate)
        {
            if (rundate == DateTime.MinValue || rundate == null)
                rundate = DateTime.Now.Date;

            List<MonikerCheckResult> result = await _gateway.GetMonikers("marketdata", "verifydefaultlist/" + rundate.Value.ToString("yyyy-MM-dd"));

            var model = new MissingMonikerModel("MissingMonikers")
            {
                MissingMonikers = result,
                RunDate = rundate.Value.Date.ToString("yyyy-MM-dd")
            };

            return View("MissingMonikers", model);
        }
    }
}