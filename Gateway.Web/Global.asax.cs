using Absa.Cib.JwtAuthentication.Extensions;
using Absa.Cib.MIT.TaskScheduling.Client;
using Absa.Cib.MIT.TaskScheduling.Models;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.IoC.Models;
using Bagl.Cib.MIT.IoC.Service;
using Bagl.Cib.MIT.Logging;
using CommonServiceLocator;
using Gateway.Web.Authorization;
using Gateway.Web.Controllers;
using Gateway.Web.ModelBindersConverters;
using Gateway.Web.Models.Export;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Models.Security;
using Gateway.Web.Services.Schedule.Utils;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Unity;
using Unity.ServiceLocation;

namespace Gateway.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public const string ContainerKey = "container";
        private static UnityContainer _container;

        public static string ControllerIcon { get; set; }
        public static string Environment { get; set; }
        public static string FavIcon { get; set; }
        public static string SigmaHomePage { get; set; }
        public static string SiteLogo { get; set; }
        public static string MetricsUrlTemplate { get; set; }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var childContainer = _container.CreateChildContainer();
            HttpContext.Current.Items[ContainerKey] = childContainer;
            var resolver = new UnityDependencyResolver(childContainer);
            DependencyResolver.SetResolver(resolver);
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            var container = HttpContext.Current.Items[ContainerKey] as IUnityContainer;
            if (container == null) return;
            container.Dispose();
            HttpContext.Current.Items.Remove(ContainerKey);
        }

        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            var loggingService = ServiceLocator.Current.GetInstance<ILoggingService>();
            loggingService.GetLogger(this).Error(ex, "Application Exception");
            loggingService.GetLogger(this).Error(ex.InnerException, "Application InnerException");
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            Environment = ConfigurationManager.AppSettings["Environment"];
            MetricsUrlTemplate = ConfigurationManager.AppSettings["BatchMetricsUrl"];
            FavIcon = "~/content/img/favicon." + Environment + ".png";
            ControllerIcon = "~/content/img/controller." + Environment + ".png";
            SiteLogo = "~/Content/img/Redstone." + Environment + ".png";

            AreaRegistration.RegisterAllAreas();

            // Add handle error attribute and authorize attribute to entire site.
            GlobalFilters.Filters.Add(new HandleErrorAttribute());
            //GlobalFilters.Filters.Add(new RoleBasedAuthorizeAttribute());
            //GlobalFilters.Filters.Add(new GatewayAuthenticationFilter());

            ModelBinders.Binders[typeof(ScheduleWebRequestModel)] = new ScheduleWebRequestModelBinder();
            ModelBinders.Binders[typeof(ScheduleBatchModel)] = new ScheduleBatchModelBinder();
            ModelBinders.Binders[typeof(SourceInformationViewModel)] = new SourceInformationViewModelBinders();
            ModelBinders.Binders[typeof(DestinationInfoViewModel)] = new DestinationInformationViewModelBinders();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles, Environment);

            BundleTable.EnableOptimizations = true;
            _container = new UnityContainer();
            var information = new SystemInformation("Redstone.UI", Environment, SessionKeyType.Application, new string[0], _container);

            CentralConfigurationService.ApplyCentralConfiguration(information);

            var dns = information.GetSetting("DnsName", "sigma.absa.co.za");

            var authurl = $"https://{dns}";
            var issuers = Issuer.GetIssuers(information);
            if (issuers.Any())
            {
                if (Debugger.IsAttached)
                    dns = "sigma-dev.absa.co.za";
                authurl = issuers.FirstOrDefault(i => i.Contains(dns)) ?? "sigma.absa.co.za";
                if (Debugger.IsAttached)
                    dns = information.GetSetting("DnsName", "sigma.absa.co.za");
            }

            BatchRequestBuilderEx.AuthUrl = authurl;
            BatchRequestBuilderEx.BaseUrl = information.GetSetting("redstonebaseurl", "https://abcap-foutils.intra.absa.co.za:7010/") + "Api/";
            BatchRequestBuilderEx.AuthQuery = information.GetSetting("AuthQuery", "authorization/oauth/token");

            information.AddSetting("LocalGateway", dns);

            Registrations.Register(information);

            var loggingservice = _container.Resolve<ILoggingService>();
            loggingservice.Initialize(information.LoggingInformation);
            var logger = loggingservice.GetLogger(this);

            var systemInformation = _container.Resolve<ISystemInformation>();
            var database = systemInformation.GetSetting("GatewayDatabase");
            var server = systemInformation.GetSetting("DatabaseServer");
            var schedulingConnectionString = $"data source={server};initial catalog={database};integrated security=True;multipleactiveresultsets=True;application name=EntityFramework";
            SigmaHomePage = $"https://{dns}";
            if (Debugger.IsAttached)
                SigmaHomePage = $"https://sigma-dev.absa.co.za";

            logger.Info($"SchedulingClientProvider : Using connection string:{schedulingConnectionString}");
            var schedulingClientProvider = new SchedulingClientProvider();
            schedulingClientProvider.Setup(new SchedulingClientOptions
            {
                SqlServerConnectionString = schedulingConnectionString,
                SystemInformation = information
            });

            var locator = new UnityServiceLocator(_container);
            ServiceLocator.SetLocatorProvider(() => locator);
            DependencyResolver.SetResolver(new UnityDependencyResolver(_container));

            PopulateDynamicReports(information);
        }

        private void PopulateDynamicReports(ISystemInformation information)
        {
            var index = 1;
            do
            {
                string value;
                if (!information.TryGetSetting($"SecurityReport{index}", out value))
                    break;

                var report = new DynamicSecurityReport(value);
                SecurityController.DyanmicReports.Add(report);
                index++;
            } while (true);
        }
    }
}