using System.Web.Hosting;
using System.Web.Services.Description;
using Gateway.Web;
using log4net.Repository.Hierarchy;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof (Startup))]

namespace Gateway.Web
{
    public class Startup
    {
        public static string Environment { get; set; }

        public void Configuration(IAppBuilder app)
        {
            var config = new HubConfiguration();
            //TODO: Pass config to SignalR - config will contain dependencies that will be 
            //injected into Hubs
            app.MapSignalR();
        }

        public void ConfigureServices(ServiceCollection services)
        {
        }

        public void Configure(IAppBuilder app, HostingEnvironment env, ILoggerFactory loggerFactory)
        {
        }
    }
}