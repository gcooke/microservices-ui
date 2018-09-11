using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Absa.Cib.MIT.TaskScheduling.Client;
using Absa.Cib.MIT.TaskScheduling.Models;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.IoC.Models;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;
using Gateway.Web.ModelBindersConverters;
using Gateway.Web.Models.Schedule.Input;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;

namespace Gateway.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static UnityContainer _container;

        public static string Environment { get; set; }
        public static string FavIcon { get; set; }
        public static string ControllerIcon { get; set; }
        public static string SiteLogo { get; set; }

        protected void Application_Start(object sender, EventArgs e)
        {
            Environment = ConfigurationManager.AppSettings["Environment"];
            FavIcon = "~/content/img/favicon." + Environment + ".png";
            ControllerIcon = "~/content/img/controller." + Environment + ".png";
            SiteLogo = "~/Content/img/Redstone." + Environment + ".png";

            AreaRegistration.RegisterAllAreas();

            // Add handle error attribute and authorize attribute to entire site.
            GlobalFilters.Filters.Add(new HandleErrorAttribute());
            GlobalFilters.Filters.Add(new RoleBasedAuthorizeAttribute());
            GlobalFilters.Filters.Add(new GatewayAuthenticationFilter());

            ModelBinders.Binders[typeof(ScheduleWebRequestModel)] = new ScheduleWebRequestModelBinder();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles, Environment);

            BundleTable.EnableOptimizations = true;
            _container = new UnityContainer();
            var information = new SystemInformation("Redstone.UI", Environment, SessionKeyType.Application,
                new string[0], _container);            
            Registrations.Register(information);
            _container.Resolve<ILoggingService>().Initialize(information.LoggingInformation);
            
            var systemInformation = _container.Resolve<ISystemInformation>();
            var schedulingConnectionString = systemInformation.GetSetting("SchedulingConnectionString");

            var schedulingClientProvider = new SchedulingClientProvider();
            schedulingClientProvider.Setup(new SchedulingClientOptions
            {
                SqlServerConnectionString = schedulingConnectionString
            });
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var childContainer = _container.CreateChildContainer();
            HttpContext.Current.Items["container"] = childContainer;
            var resolver = new UnityDependencyResolver(childContainer);
            DependencyResolver.SetResolver(resolver);

            var locator = new UnityServiceLocator(_container);
            ServiceLocator.SetLocatorProvider(() => locator);
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            var container = HttpContext.Current.Items["container"] as IUnityContainer;
            if (container == null) return;
            container.Dispose();
            HttpContext.Current.Items.Remove("container");
        }
    }
}