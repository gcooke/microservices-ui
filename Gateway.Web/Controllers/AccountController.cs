using Absa.Cib.Authorization.Extensions;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MSF.ClientAPI.Provider;
using Gateway.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using ClaimTypes = Absa.Cib.Authorization.Extensions.ClaimTypes;


namespace Gateway.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _autUrl;
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly IAuthorizationProvider _authorizationProvider;

        public AccountController(
            ISystemInformation information,
            IAuthenticationProvider authenticationProvider,
            IAuthorizationProvider authorizationProvider
            )
        {
            _autUrl = information.GetSetting<string>("AuthUrl", "https://sigma-dev.absa.co.za/authorization/webauth");
            _authenticationProvider = authenticationProvider;
            _authorizationProvider = authorizationProvider;
        }

        [AllowAnonymous]
        public ActionResult LogOn(string returnUrl)
        {
#if DEBUG
            return GetTokenAndCreateAuthCookieForDebug(returnUrl);
#endif
            return RedirectToCreateAuthCookie(returnUrl);
        }

        private ActionResult RedirectToCreateAuthCookie(string returnUrl)
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

        private ActionResult GetTokenAndCreateAuthCookieForDebug(string returnUrl)
        {
            var context = HttpContext.Request.GetOwinContext();
            var authenticationManager = context.Authentication;

            var claimIdentity = GetJwtIdentity();
            authenticationManager.SignIn(claimIdentity);

            return Redirect(returnUrl);
        }

        private ClaimsIdentity GetJwtIdentity()
        {
            _authenticationProvider.Authenticate();
            var token = _authenticationProvider.GetToken();
            var userNames = _authorizationProvider.GetClaims<string>(token, ClaimTypes.Username);
            var claimsIdentity = new ClaimsIdentity(new JwtIdentity(true, userNames.FirstOrDefault(), "ApplicationCookie"));
            claimsIdentity.AddClaim(new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userNames.FirstOrDefault()));

            var claims = GetAllClaimsFromToken(token);
            claimsIdentity.AddClaims(claims);

            return claimsIdentity;
        }

        private List<Claim> GetAllClaimsFromToken(string token)
        {
            var claims = new List<Claim>();
            claims.AddRange(GetClaims(token, ClaimTypes.Role));
            claims.AddRange(GetClaims(token, ClaimTypes.Site));
            claims.AddRange(GetClaims(token, ClaimTypes.Portfolio));
            claims.AddRange(GetClaims(token, ClaimTypes.Groups));

            return claims;
        }

        private List<Claim> GetClaims(string token, ClaimTypes claimType)
        {
            var claims = new List<Claim>();

            var strClaims = _authorizationProvider.GetClaims<string>(token, claimType);

            var strClaimType = claimType == ClaimTypes.Role
                ? System.Security.Claims.ClaimTypes.Role.ToString()
                : claimType.ToString();

            foreach (var strClaim in strClaims)
            {
                claims.Add(new Claim(strClaimType, strClaim));
            }

            return claims;
        }
    }
}