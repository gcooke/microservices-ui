using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;
using Gateway.Web.Database;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Request;
using Gateway.Web.Services;
using Gateway.Web.Utils;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class RequestController : BaseController
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;

        public RequestController(IGatewayDatabaseService dataService, IGatewayService gateway, ILoggingService loggingService)
            : base(loggingService)
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

        public ActionResult Children(string correlationId, string sortOrder, string filter = "")
        {
            if (string.IsNullOrEmpty(sortOrder))
                sortOrder = "time_desc";

            ViewBag.SortColumn = sortOrder;
            ViewBag.SortDirection = sortOrder.EndsWith("_desc") ? "" : "_desc";
            ViewBag.Controller = "Request";
            ViewBag.Action = "Children";

            var model = new Children();
            var id = string.IsNullOrEmpty(correlationId) ? (Guid)Session["LastCorrelationId"] : Guid.Parse(correlationId);
            Session["LastCorrelationId"] = id;
            model.CorrelationId = id;

            IEnumerable<HistoryItem> items = _dataService.GetRequestChildren(id);
            if (!string.IsNullOrEmpty(filter))
            {
                // Only include requests that match the controller name.
                items = items.Where(r => string.Equals(r.Controller, filter, StringComparison.CurrentCultureIgnoreCase));
            }

            model.Requests.AddRange(items, sortOrder);
            model.Requests.SetRelativePercentages();

            model.Requests.SetRelativePercentages();
            return View(model);
        }

        [RoleBasedAuthorize(Roles = "Security.Modify")]
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
            return File(data.GetBytes(), "text/xml", string.Format("Payload_{0}.txt", payloadId));
        }

        public ActionResult DownloadCube(string correlationId, long payloadId)
        {
            var data = _dataService.GetPayload(payloadId);
            return File(data.GetCubeBytes(), "application/octet-stream", string.Format("Cube_{0}.dat", payloadId));
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