using System;
using Gateway.Web;
using log4net.Repository.Hierarchy;
using Microsoft.Owin;
using Owin;
using System.Web.Services.Description;
using System.Web.Hosting;

[assembly: OwinStartup(typeof(Startup))]

namespace Gateway.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
        }
        public void ConfigureServices(ServiceCollection services)
        {
            // Code omitted
            services.AddTask<FeedEngine>();
        }
        public void Configure(IAppBuilder app, HostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Code omitted
            app.StartTask<FeedEngine>(TimeSpan.FromSeconds(15));
        }
    }
}