using Absa.Cib.Authorization.Extensions;
using Bagl.Cib.MIT.IoC;
using Gateway.Web.Authorization;
using Gateway.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Unity;

namespace Gateway.Web.Controllers
{
    public class AccountController : Controller
    {
        private string _autUrl = "http://localhost:59782";
       

        public AccountController(ISystemInformation information)
        {
            _autUrl = information.GetSetting<string>("AuthUrl");
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

            var redirectUrl = Request.Url.ToString().Substring(0, Request.Url.ToString().LastIndexOf('/'));
            redirectUrl = redirectUrl + "/ProcessRedirect";

            var url = $"{_autUrl}?redirect={redirectUrl}&page={returnUrl}";
          

            return Redirect(url);
        }


        [AllowAnonymous]
        public ActionResult ProcessRedirect()
        {
            var page = Request.QueryString["page"];
            var context = HttpContext.Request.GetOwinContext();
            var authenticationManager = context.Authentication;

            if (string.IsNullOrEmpty(page))
            {
                return Redirect("~/Error/Unauthorized");
            }

            var jwtToken = Request["SIGMA_AUTH"];

            var container = HttpContainerProvider.Container;
          
            var authorize = container.Resolve<IAuthorizationProvider>();
           var userName = authorize.GetClaims<string>(jwtToken,Absa.Cib.Authorization.Extensions.ClaimTypes.Username)?.FirstOrDefault();
            var roles = authorize.GetClaims<string>(jwtToken, Absa.Cib.Authorization.Extensions.ClaimTypes.Role) ?? new List<string>();


            var jwtIdentity = new ClaimsIdentity(new Models.Auth.JwtIdentity(true, userName, "ApplicationCookie"));

            jwtIdentity.AddClaim(new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userName));

            foreach (var role in roles)
            {
                 jwtIdentity.AddClaim(new Claim(System.Security.Claims.ClaimTypes.Role, role));
            }

            authenticationManager.SignIn(jwtIdentity);

            return Redirect(page);

        }
    }

    
}