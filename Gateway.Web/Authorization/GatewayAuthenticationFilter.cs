using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using Bagl.Cib.MSF.ClientAPI.Provider;

namespace Gateway.Web.Authorization
{
    public class GatewayAuthenticationFilter : IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new HttpUnauthorizedResult();
                return;
            }

            var controller = (Controller)filterContext.Controller;
            var authenticationProvider = controller.Resolver.GetService(typeof(IAuthenticationProvider)) as IAuthenticationProvider;

            if (authenticationProvider == null)
                throw new Exception("Unable to find instance of IAuthenticationProvider.");

            var requestCookie = filterContext.HttpContext.Request.Cookies["X-Token"];

            if (string.IsNullOrWhiteSpace(requestCookie?.Value))
            {
                authenticationProvider.Authenticate();
                var token = authenticationProvider.GetToken();

                var httpCookie = new HttpCookie("X-Token", token) { Expires = DateTime.Now.AddHours(5) };
                filterContext.HttpContext.Response.Cookies.Add(httpCookie);
                return;
            }

            authenticationProvider.SetToken(requestCookie.Value);
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
        }
    }
}