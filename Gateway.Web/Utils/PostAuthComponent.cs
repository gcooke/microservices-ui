using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using Absa.Cib.JwtAuthentication;
using Unity;
using System.Security.Claims;
using Bagl.Cib.MSF.ClientAPI.Provider;

namespace Gateway.Web.Utils
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class PostAuthComponent
    {
        private AppFunc _next;

        public PostAuthComponent(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var context = HttpContext.Current.GetOwinContext();

            if (context.Authentication.User != null && context.Authentication.User.Identity.IsAuthenticated)
            {
                var container = HttpContext.Current.Items[MvcApplication.ContainerKey] as IUnityContainer;

                SetAuthenticationToken(container, context.Authentication.User.Identity.Name, context.Authentication.User.Claims);
            }


            await _next.Invoke(environment);
        }

        private void SetAuthenticationToken(IUnityContainer container, string user, IEnumerable<Claim> owinClaims)
        {

            string jwtToken = HttpContext.Current.Cache[$"jwt-{user}"]?.ToString();

            if (string.IsNullOrEmpty(jwtToken))
            {

                var claims = GetClaims(owinClaims);
                jwtToken = GetJWTToken(container, claims);
                HttpContext.Current.Cache.Add($"jwt-{user}", jwtToken, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 15, 0), CacheItemPriority.Normal, null);
            }


            var authenticationProvider = container.Resolve<IAuthenticationProvider>();
            authenticationProvider.SetToken(jwtToken);
        }

        private string GetJWTToken(IUnityContainer container, IList<Claim> claims)
        {
            var jwtService = container.Resolve<IJwtClaimsService>();
            var token = jwtService.GetTokenFromClaims(claims);
            return token;
        }

        private IList<Claim> GetClaims(IEnumerable<Claim> owinClaims)
        {
            var jwtClaims = new List<Claim>();

            foreach (var owinClaim in owinClaims)
            {
                if (owinClaim.Type == ClaimTypes.NameIdentifier)

                    jwtClaims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, owinClaim.Value));
                else if (owinClaim.Type == ClaimTypes.Role)
                    jwtClaims.Add(new Claim("Role", owinClaim.Value));
                else
                    jwtClaims.Add(owinClaim);
            }

            return jwtClaims;
        }
    }
}