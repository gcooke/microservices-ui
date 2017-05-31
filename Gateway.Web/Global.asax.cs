using System.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Bagl.Cib.MIT.IoC.Models;
using Bagl.Cib.MIT.Logging;
using Microsoft.Practices.Unity;

namespace Gateway.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static string Environment { get; set; }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Environment = ConfigurationManager.AppSettings["Environment"];
            var container = new UnityContainer();
            var information = new SystemInformation("Redstone.UI", Environment, SessionKeyType.Application, new string[0], container);
            Registrations.Register(information);
            container.Resolve<ILoggingService>().Initialize(information.LoggingInformation);
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }        
    }
}
