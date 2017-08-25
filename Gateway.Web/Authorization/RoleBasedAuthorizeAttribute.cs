using System.Linq;
using System.Web.Mvc;

namespace Gateway.Web.Authorization
{
    public class RoleBasedAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // The user is not authenticated
                base.HandleUnauthorizedRequest(filterContext);
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
    }
}