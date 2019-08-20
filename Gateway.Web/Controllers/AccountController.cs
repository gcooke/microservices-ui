using Bagl.Cib.MIT.IoC;
using System.Web;
using System.Web.Mvc;

namespace Gateway.Web.Controllers
{
    public class AccountController : Controller
    {
        private string _autUrl;

        public AccountController(ISystemInformation information)
        {
            _autUrl = information.GetSetting<string>("AuthUrl", "https://sigma-dev.absa.co.za/authorization/webauth");
        }

        [AllowAnonymous]
        public ActionResult LogOn(string returnUrl)
        {
            var context = HttpContext.Request.GetOwinContext();
            var authenticationManager = context.Authentication;

            if (authenticationManager.User.Identity.IsAuthenticated)
            {
                return Redirect("~/Error/Unauthorized");
            }

            var pathQuery = context.Request.Uri.PathAndQuery;
            var redirectUrl = context.Request.Uri.AbsoluteUri.Replace(pathQuery, $"/{returnUrl}");

            var url = $"{_autUrl}?redirect={redirectUrl}";

            return Redirect(url);
        }
    }
}