using System.Linq;
using System.Web.Mvc;
using Bagl.Cib.MIT.IoC;
using CommonServiceLocator;

namespace Gateway.Web.Authorization
{
    public class RoleBasedAuthorizeAttribute : AuthorizeAttribute
    {
        private static string _environment;

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // The user is not authenticated
                base.HandleUnauthorizedRequest(filterContext);
            }
            else if (string.Equals(GetEnvironmentName(), "DEV"))
            {
                // Allow all users access to DEV
            }
            else if (!this.Roles.Split(',').Any(filterContext.HttpContext.User.IsInRole))
            {
                // The user is not in any of the listed roles => 
                // show the unauthorized view



                filterContext.Result = new RedirectResult("~/Error/Unauthorized");
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }

        private string GetEnvironmentName()
        {
            if (!string.IsNullOrEmpty(_environment)) return _environment;

            var information = ServiceLocator.Current.GetInstance<ISystemInformation>();
            _environment = information.EnvironmentName;
            return information.EnvironmentName;
        }
    }
}