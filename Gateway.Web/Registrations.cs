using System;
using Absa.Cib.MIT.TaskScheduling.Client.Scheduler;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.IO;
using Bagl.Cib.MIT.IO.Impl;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MIT.Logging.Impl;
using Bagl.Cib.MIT.Redis;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Bagl.Cib.MSF.ClientAPI.Provider;
using Gateway.Web.Database;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services;
using Gateway.Web.Services.Batches;
using Gateway.Web.Services.Batches.Interfaces;
using StackExchange.Redis;
using Gateway.Web.Services.Monitoring.RiskReports;
using Gateway.Web.Services.Monitoring.ServerDiagnostics;
using Gateway.Web.Services.Schedule;
using Gateway.Web.Services.Schedule.Interfaces;
using System.Web.Mvc;
using Gateway.Web.Controllers;

namespace Gateway.Web
{
    public static class Registrations
    {
        public static void Register(ISystemInformation information)
        {
            //Controller Registrations
            information.RegisterType<AddInController>(Scope.ContainerSingleton);
            information.RegisterType<BaseController>(Scope.ContainerSingleton);
            information.RegisterType<BatchController>(Scope.ContainerSingleton);
            information.RegisterType<BatchScheduleController>(Scope.ContainerSingleton);
            information.RegisterType<ControllerController>(Scope.ContainerSingleton);
            information.RegisterType<ControllersController>(Scope.ContainerSingleton);
            information.RegisterType<DownloadsController>(Scope.ContainerSingleton);
            information.RegisterType<GroupController>(Scope.ContainerSingleton);
            information.RegisterType<HomeController>(Scope.ContainerSingleton);
            information.RegisterType<MarketDataController>(Scope.ContainerSingleton);
            information.RegisterType<MonitoringController>(Scope.ContainerSingleton);
            information.RegisterType<PermissionController>(Scope.ContainerSingleton);
            information.RegisterType<ReleaseProcessController>(Scope.ContainerSingleton);
            information.RegisterType<RequestController>(Scope.ContainerSingleton);
            information.RegisterType<RequestScheduleController>(Scope.ContainerSingleton);
            information.RegisterType<ScheduleController>(Scope.ContainerSingleton);
            information.RegisterType<SecurityController>(Scope.ContainerSingleton);
            information.RegisterType<ServerResourceController>(Scope.ContainerSingleton);
            information.RegisterType<UserController>(Scope.ContainerSingleton);
            information.RegisterType<UtilsController>(Scope.ContainerSingleton);

            // Setup Logging
            information.LoggingInformation.LogPath = information.GetSetting("LogLocation");
            information.LoggingInformation.FileLogLevel.SetLogLevel(information.GetSetting("ConsoleLogLevel"));
            information.LoggingInformation.FileLogLevel.SetLogLevel(information.GetSetting("FileLogLevel"));
            information.LoggingInformation.ImportantLogLevel.SetLogLevel(information.GetSetting("ImportantLogLevel"));
            information.RegisterType<ILogFileService, LogFileService>(Scope.Singleton);
            information.RegisterType<ILoggingService, DefaultLoggingService>(Scope.Singleton);
            information.RegisterType<ILogsService, LogsService>(Scope.Singleton);


            //Register Redis 
            information.RegisterType<IRedisConnectionProvider, RedisConnectionProvider>(Scope.Singleton);

            var redisOptions = new ConfigurationOptions()
            {
                ClientName = AppDomain.CurrentDomain.FriendlyName,
                EndPoints = { information.GetSetting("Redis.ConnectionStr", "localhost") },
                ConnectTimeout = 20000,
                SyncTimeout = 10000,
                AllowAdmin = false,
                DefaultDatabase = 0
            };

            var options = new RedisConfiguration()
            {
                RedisOptions = redisOptions,
                DefaultExpiration = TimeSpan.FromMinutes(10)
            };

            information.RegisterInstance(options, Scope.Singleton);


            information.RegisterType<IGatewayDatabaseService, GatewayDatabaseService>(Scope.Singleton);            
            information.RegisterType<IDateTimeProvider, DateTimeProvider>(Scope.Singleton);
            information.RegisterType<IFileService, FileService>(Scope.Singleton);
            information.RegisterType<IDifferentialArchiveService, DifferentialArchiveService>(Scope.Singleton);
            information.RegisterType<IDifferentialDownloadService, DifferentialDownloadService>(Scope.Singleton);
            information.RegisterType<IDateTimeProvider, DateTimeProvider>(Scope.Singleton);         
            
            information.RegisterType<IActiveDirectoryService, ActiveDirectoryService>(Scope.Singleton);
            information.RegisterType<IBatchConfigService, BatchConfigService>(Scope.Singleton);
            information.RegisterType<IScheduleDataService, ScheduleDataService>(Scope.Singleton);
            information.RegisterType<IRedstoneWebRequestScheduler, RedstoneWebRequestScheduler>(Scope.Singleton);
            information.RegisterType<IExecutableScheduler, ExecutableScheduler>(Scope.Singleton);
            information.RegisterType<IBatchConfigDataService, BatchConfigDataService>(Scope.Singleton);
            information.RegisterType<IScheduleService<ScheduleBatchModel>, RedstoneBatchScheduleService>(Scope.Singleton);
            information.RegisterType<IScheduleService<ScheduleWebRequestModel>, RedstoneRequestScheduleService>(Scope.Singleton);
            information.RegisterType<IScheduleService<ScheduleExecutableModel>, ExecutableScheduleService>(Scope.Singleton);
            information.RegisterType<IScheduleGroupService, ScheduleGroupService>(Scope.Singleton);
            information.RegisterType<IRequestConfigurationService, RequestConfigurationService>(Scope.Singleton);
            information.RegisterType<IHttpClientProvider, HttpClientProvider>(Scope.Singleton);

            //Reset Services Registrations
            information.RegisterType<IRestService, RestService>(Scope.ContainerSingleton);
            information.RegisterType<IRoleService, RoleService>(Scope.ContainerSingleton);
            information.RegisterType<IRiskReportMonitoringService, RiskReportMonitoringService>(Scope.ContainerSingleton);
            information.RegisterType<IGateway, Bagl.Cib.MSF.ClientAPI.Gateway.Gateway>(Scope.ContainerSingleton);
            information.RegisterType<ISimpleRestService, SimpleRestService>(Scope.ContainerSingleton);
            information.RegisterType<IParentRequestDetailProvider, ParentRequestDetailProvider>(Scope.ContainerSingleton);
            information.RegisterType<IAuthenticationProvider, AuthenticationProvider>(Scope.ContainerSingleton);
            information.RegisterType<IImpersonationProvider, ImpersonationProvider>(Scope.ContainerSingleton);
            information.RegisterType<IGatewayService, GatewayService>(Scope.ContainerSingleton);
            information.RegisterType<IBasicRestService, BasicRestService>(Scope.ContainerSingleton);
            information.RegisterType<IGatewayRestService, GatewayRestService>(Scope.ContainerSingleton);
            information.RegisterType<IServerDiagnosticsService, ServerDiagnosticsService>(Scope.ContainerSingleton);


            Absa.Cib.Authorization.Extensions.Registration.Register(information);
            Absa.Cib.Authorization.Extensions.Registration.RegisterCertificates(information);
            Absa.Cib.JwtAuthentication.Registrations.Register(information);
            Absa.Cib.JwtAuthentication.Registrations.RegisterCertificates(information);
        }


        private static void SetupLogging(ISystemInformation information)
        {

        }

        private static void SetLogLevel(this LogLevel value, string logLevel)
        {
            LogLevel level;
            value = (Enum.TryParse(logLevel, out level)) ? level : value;
        }
    }
}