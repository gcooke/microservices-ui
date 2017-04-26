using System.Web.Mvc;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    public class UtilsController : Controller
    {
        public UtilsController()
        {
        }

        public ActionResult Errors()
        {
            return View();
        }

        public ActionResult Glyphs()
        {
            return View();
        }        
    }
}