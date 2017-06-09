using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Services;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    public class RequestController : Controller
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;

        public RequestController(IGatewayDatabaseService dataService, IGatewayService gateway)
        {
            _dataService = dataService;
            _gateway = gateway;
        }

        public ActionResult Detail(string correlationId)
        {
            var model = _dataService.GetRequestDetails(correlationId);
            return View(model);
        }

        public ActionResult Cancel(string correlationId)
        {
            _gateway.ExpireWorkItem(correlationId);

            // This is a crude approach to waiting for the audit to be written prior to refreshing the page.
            System.Threading.Thread.Sleep(2000);

            return Redirect("~/Request/Detail?correlationId=" + correlationId);
        }

        public ActionResult Download(string correlationId, long payloadId)
        {
            var data = _dataService.GetPayload(payloadId);
            return File(data.GetBytes(), "text/plain", string.Format("Payload_{0}.txt", payloadId));
        }

        public ActionResult Timings(string correlationId)
        {
            return View();
        }
    }
}