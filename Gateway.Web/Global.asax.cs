﻿
using System.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Bagl.Cib.MIT.IoC.Models;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;

namespace Gateway.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static string Environment { get; set; }
        public static string FavIcon { get; set; }
        public static string ControllerIcon { get; set; }
        public static string SiteLogo { get; set; }

        protected void Application_Start()
        {
            Environment = ConfigurationManager.AppSettings["Environment"];
            FavIcon = "~/content/img/favicon." + Environment + ".png";
            ControllerIcon = "~/content/img/controller." + Environment + ".png";
            SiteLogo = "~/Content/img/Redstone." + Environment + ".png";

            AreaRegistration.RegisterAllAreas();

            // Add handle error attribute and authorize attribute to entire site.
            GlobalFilters.Filters.Add(new HandleErrorAttribute());
            GlobalFilters.Filters.Add(new RoleBasedAuthorizeAttribute());

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles, Environment);

            BundleTable.EnableOptimizations = true;
            var container = new UnityContainer();
            var information = new SystemInformation("Redstone.UI", Environment, SessionKeyType.Application,
                new string[0], container);
            Registrations.Register(information);
            container.Resolve<ILoggingService>().Initialize(information.LoggingInformation);
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
            var locator = new UnityServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => locator);
        }
    }
}
