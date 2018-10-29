using System;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Absa.Cib.MIT.TaskScheduling.Client;
using Absa.Cib.MIT.TaskScheduling.Models;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.IoC.Models;
using Bagl.Cib.MIT.IoC.Service;
using Bagl.Cib.MIT.Logging;
using CommonServiceLocator;
using Gateway.Web.Authorization;
using Gateway.Web.ModelBindersConverters;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services.Schedule.Utils;
using Unity;
using Unity.ServiceLocation;

namespace Gateway.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static UnityContainer _container;

        public static string Environment { get; set; }
        public static string FavIcon { get; set; }
        public static string ControllerIcon { get; set; }
        public static string SiteLogo { get; set; }

        public static string SigmaHomePage { get; set; }

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
            var information = new SystemInformation("Redstone.UI", Environment, SessionKeyType.Application, new string[0], _container);

            CentralConfigurationService.ApplyCentralConfiguration(information);

            var dns = information.GetSetting("DnsName", "abcap-foutils.intra.absa.co.za");
            var authurl = $"https://{dns}";

            BatchRequestBuilderEx.AuthUrl = authurl;
            BatchRequestBuilderEx.BaseUrl = information.GetSetting("redstonebaseurl", "https://abcap-foutils.intra.absa.co.za:7010/")+"Api/";
            BatchRequestBuilderEx.AuthQuery = information.GetSetting("AuthQuery", "authorization/oauth/token");

            information.AddSetting("LocalGateway", dns);


            Registrations.Register(information);
            _container.Resolve<ILoggingService>().Initialize(information.LoggingInformation);
            
            var systemInformation = _container.Resolve<ISystemInformation>();
            var schedulingConnectionString = systemInformation.GetSetting("SchedulingConnectionString");

;
            SigmaHomePage = $"https://{dns}";

            var schedulingClientProvider = new SchedulingClientProvider();
            schedulingClientProvider.Setup(new SchedulingClientOptions
            {
                SqlServerConnectionString = schedulingConnectionString
            });

            var locator = new UnityServiceLocator(_container);
            ServiceLocator.SetLocatorProvider(() => locator);
            DependencyResolver.SetResolver(new UnityDependencyResolver(_container));
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var childContainer = _container.CreateChildContainer();
            HttpContext.Current.Items["container"] = childContainer;
            var resolver = new UnityDependencyResolver(childContainer);
            DependencyResolver.SetResolver(resolver);
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