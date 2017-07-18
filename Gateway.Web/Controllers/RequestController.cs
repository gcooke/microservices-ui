using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models.Request;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    public class RequestController : Controller
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;
        private const string CurrentRequestCorrelationId = "CurrentRequestCorrelationId";


        public RequestController(IGatewayDatabaseService dataService, IGatewayService gateway)
        {
            _dataService = dataService;
            _gateway = gateway;
        }

        public ActionResult Summary(string correlationId)
        {
            var model = _dataService.GetRequestSummary(correlationId);
            return View(model);
        }

        public ActionResult Payloads(string correlationId)
        {
            var model = _dataService.GetRequestPayloads(correlationId);
            return View(model);
        }

        public ActionResult Transitions(string correlationId)
        {
            var model = _dataService.GetRequestTransitions(correlationId);
            return View(model);
        }

        public ActionResult Children(string correlationId, string filter = "")
        {
            Session.RegisterLastHistoryLocation(Request.Url);

            var model = _dataService.GetRequestChildren(correlationId);
            if (!string.IsNullOrEmpty(filter))
            {
                // Remove all requests that don't match the controller name.
                model.Requests.RemoveAll(
                    r => !string.Equals(r.Controller, filter, StringComparison.CurrentCultureIgnoreCase));
            }
            model.Requests.SetRelativePercentages();
            return View(model);
        }

        public ActionResult Cancel(string correlationId)
        {
            _gateway.ExpireWorkItem(correlationId);

            // This is a crude approach to waiting for the audit to be written prior to refreshing the page.
            System.Threading.Thread.Sleep(2000);

            return Redirect("~/Request/Summary?correlationId=" + correlationId);
        }

        public ActionResult Download(string correlationId, long payloadId)
        {
            var data = _dataService.GetPayload(payloadId);
            return File(data.GetBytes(), "text/plain", string.Format("Payload_{0}.txt", payloadId));
        }

        public ActionResult Timings(string id)
        {
            Guid correlationId;
            if (!Guid.TryParse(Convert.ToString(id), out correlationId))
                return View();

            var payload = _gateway.GetRequestTree(correlationId);
            var model = new Timings(payload);
            return View(model);
        }
    }
}