using Absa.Cib.JwtAuthentication;
using Bagl.Cib.MSF.ClientAPI.Provider;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using Unity;

namespace Gateway.Web.Authorization
{
    public class GatewayAuthenticationFilter : IAuthenticationFilter
    {
        private static string _claimKeyNameSpace = "{Security}";

        public void OnAuthentication(AuthenticationContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new HttpUnauthorizedResult();
                return;
            }

            var container = (IUnityContainer)context.HttpContext.Items["container"];
            var authenticationProvider = container.Resolve<IAuthenticationProvider>();

            if (authenticationProvider == null)
            {
                var message = "Unable to find instance of IAuthenticationProvider.";
                Log(context, message);
                throw new Exception(message);
            }

            var requestCookie = context.HttpContext.Request.Cookies["SIGMA_AUTH"];
            if (string.IsNullOrWhiteSpace(requestCookie?.Value))
            {
                var jwtClaimsService = container.Resolve<IJwtClaimsService>();

                if (jwtClaimsService == null)
                {
                    Log(context, "Unable to find instance of IJwtClaimsService.");
                    throw new ArgumentNullException("IJwtClaimsService");
                }
                
                var message = "Trying to get token for user " + context.HttpContext.User.Identity.Name;
                Log(context, message);

                try
                {
                    var tokenTask = jwtClaimsService.GetClaimsToken(context.HttpContext.User.Identity.Name);

                    if (!tokenTask.Wait(TimeSpan.FromSeconds(30)))
                    {
                        throw new Exception("Unable to get token from data services.");
                    }

                    var token = tokenTask.Result.Replace(@"""", "");
                    authenticationProvider.SetToken(token);
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

                    var httpCookie = new HttpCookie("SIGMA_AUTH", token) { Expires = expiry };
                    context.HttpContext.Response.Cookies.Add(httpCookie);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
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