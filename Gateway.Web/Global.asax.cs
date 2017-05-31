using System.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Bagl.Cib.MIT.IoC.Models;
using Microsoft.Practices.Unity;

namespace Gateway.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var environment = ConfigurationManager.AppSettings["Environment"];
            var container = new UnityContainer();
            var information = new SystemInformation("Redstone.UI", environment, SessionKeyType.Application, new string[0], container);
            Registrations.Register(information);
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }        
    }
}
