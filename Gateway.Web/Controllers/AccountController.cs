using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Gateway.Web.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]

        public ActionResult LogOn(string returnUrl)
        {
            var context = HttpContext.Request.GetOwinContext();
            var authenticationManager = context.Authentication;

            if (authenticationManager.User.Identity.IsAuthenticated)
            {
               return Redirect("~/Error/Unauthorized");
            }

            var jwtIdentity = new ClaimsIdentity(new Models.Auth.JwtIdentity(true, "D_ABSA\\ABKR", "ApplicationCookie"));

            jwtIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "D_ABSA\\ABKR"));

            jwtIdentity.AddClaim(new Claim(ClaimTypes.Role, "Access1"));

            
          

            authenticationManager.SignIn(jwtIdentity);

            return Redirect(returnUrl);
        }
    }

    
}