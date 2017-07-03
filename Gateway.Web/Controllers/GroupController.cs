using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models.Group;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    public class GroupController : Controller
    {
        private readonly IGatewayService _gateway;

        public GroupController(IGatewayService gateway)
        {
            _gateway = gateway;
        }

        public ActionResult Index()
        {
            return Details("0");
        }

        public ActionResult Details(string id)
        {
            var model = _gateway.GetGroup(id.ToLongOrDefault());
            return View("Details", model);
        }

        public ActionResult ADGroups(string id)
        {
            var model = _gateway.GetGroupADGroups(id.ToLongOrDefault());
            return View("ADGroups", model);
        }

        [HttpPost]
        public ActionResult InsertGroupADGroups(FormCollection collection)
        {

            //Setup next view
            return ADGroups("0");
        }


        public ActionResult Permissions(string id)
        {
            var model = _gateway.GetGroupPermisions(id.ToLongOrDefault());
            return View(model);
        }

        public ActionResult Portfolios(string id)
        {
            var model = _gateway.GetGroupPortfolios(id.ToLongOrDefault());
            return View(model);
        }

        public ActionResult Sites(string id)
        {
            var model = _gateway.GetGroupSites(id.ToLongOrDefault());
            return View(model);
        }

        public ActionResult AddIns(string id)
        {
            var model = _gateway.GetGroupAddIns(id.ToLongOrDefault());
            return View(model);
        }
    }
}