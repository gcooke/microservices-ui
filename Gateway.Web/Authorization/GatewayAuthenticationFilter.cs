using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using Bagl.Cib.MSF.ClientAPI.Provider;

namespace Gateway.Web.Authorization
{
    public class GatewayAuthenticationFilter : IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new HttpUnauthorizedResult();
                return;
            }

            var controller = (Controller)context.Controller;
            var authenticationProvider = controller.Resolver.GetService(typeof(IAuthenticationProvider)) as IAuthenticationProvider;

            if (authenticationProvider == null)
            {
                var message = "Unable to find instance of IAuthenticationProvider.";
                Log(context, message);
                throw new Exception(message);
            }

            var requestCookie = context.HttpContext.Request.Cookies["X-Token"];
            if (string.IsNullOrWhiteSpace(requestCookie?.Value))
            {
                var message = "Trying to get token for user " + context.HttpContext.User.Identity.Name;
                Log(context, message);

                authenticationProvider.Authenticate();
                var token = authenticationProvider.GetToken();
                var expiry = DateTime.Now.AddHours(5);

                if (string.IsNullOrEmpty(token))
                {
                    message = "Token generation failed. Please see logs";
                    Log(context, message);
                    token = "INVALID";
                    expiry = DateTime.Now.AddMinutes(5);
                }
                else
                {
                    message = "Token generated: " + (token.Length > 50 ? token.Substring(0, 50) : token) + "...";
                    Log(context, message);
                }

                var httpCookie = new HttpCookie("X-Token", token) { Expires = expiry };
                context.HttpContext.Response.Cookies.Add(httpCookie);
            }

            if (requestCookie != null && requestCookie.Value != "INVALID")
                authenticationProvider.SetToken(requestCookie.Value);
        }

        private void Log(AuthenticationContext context, string message)
        {
            context.HttpContext.Response.AppendToLog(message);
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
        }
    }
}