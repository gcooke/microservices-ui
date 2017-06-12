using System;
using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models.User;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;

        public UserController(IGatewayDatabaseService dataService, IGatewayService gateway)
        {
            _dataService = dataService;
            _gateway = gateway;
        }

        public ActionResult History(string id)
        {
            Session.RegisterLastHistoryLocation(Request.Url);

            var items = _dataService.GetRecentUserRequests("INTRANET\\" + id, DateTime.Today.AddDays(-7));

            var model = new HistoryModel(id);
            model.Requests.AddRange(items);
            model.Requests.SetRelativePercentages();
            return View(model);
        }
    }
}