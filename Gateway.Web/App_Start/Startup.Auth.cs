using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web
{ 
    public partial class Startup
    {

        public void ConfigureAuth(IAppBuilder app)
        {
           

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ApplicationCookie",
                LoginPath = new PathString("/Account/LogOn")
            });
        }

    }
}