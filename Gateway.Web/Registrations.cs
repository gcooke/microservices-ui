using Absa.Cib.JwtAuthentication.Extensions;
using Absa.Cib.JwtAuthentication.Models;
using Absa.Cib.MIT.TaskScheduling.Client.Scheduler;
using Bagl.Cib.MIT.IO;
using Bagl.Cib.MIT.IO.Impl;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MIT.Redis;
using Bagl.Cib.MIT.Redis.Caching;
using Bagl.Cib.MIT.Redis.Serialization;
using Bagl.Cib.MIT.Redis.Serialization.Json;
using Bagl.Cib.MSF.ClientAPI.Provider;
using Gateway.Web.Controllers;
using Gateway.Web.Database;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services;
using Gateway.Web.Services.Batches;
using Gateway.Web.Services.Batches.Interfaces;
using Gateway.Web.Services.Batches.Interrogation.Services.BatchService;
using Gateway.Web.Services.Batches.Interrogation.Services.IssueService;
using Gateway.Web.Services.Monitoring.ServerDiagnostics;
using Gateway.Web.Services.Pdc;
using Gateway.Web.Services.Schedule;
using Gateway.Web.Services.Schedule.Interfaces;
using StackExchange.Redis;
using System;
using Unity.Injection;

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
            information.RegisterType<RbhController>(Scope.ContainerSingleton);
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

            information.RegisterType<ILogsService, LogsService>(Scope.Singleton);

            //Register Redis
            information.RegisterType<ConfigurationOptions>(Scope.Singleton, new InjectionFactory(
                e =>
                {
                    var redisConnectionStr = information.GetSetting("RedisConnection");
                    redisConnectionStr = string.IsNullOrEmpty(redisConnectionStr) ? "localhost" : redisConnectionStr;

                    var redisOptions = new ConfigurationOptions
                    {
                        EndPoints = { redisConnectionStr },
                        ConnectTimeout = 20000,
                        SyncTimeout = 10000,
                        ClientName = AppDomain.CurrentDomain.FriendlyName,
                        AbortOnConnectFail = false,
                        ReconnectRetryPolicy = new ExponentialRetry(100),
                        HighPrioritySocketThreads = true,
                        ConnectRetry = 1000
                    };

                    var redispassword = information.GetSetting("RedisPassword");
                    var enableEncryption = bool.Parse(information.GetSetting("RedisEncryption", "False"));

                    if (enableEncryption)
                    {
                        var certificate = (ICertificate)e.Resolve(typeof(ICertificate), null, null);
                        var decryptedpassword = certificate.Decrypt(redispassword);
                        redispassword = decryptedpassword;
                    }

                    redisOptions.Password = redispassword;
                    return redisOptions;
                }));

            information.RegisterType<IRedisConnectionProvider>(Scope.Singleton, new InjectionFactory(
                e =>
                {
                    var config = (RedisConfiguration)e.Resolve(typeof(RedisConfiguration), null, null);
                    return new RedisConnectionProvider(config);
                }));

            information.RegisterType<RedisConfiguration>(Scope.Singleton, new InjectionFactory(
                e =>
                {
                    var redisconfig = (ConfigurationOptions)e.Resolve(typeof(ConfigurationOptions), null, null);

                    return new RedisConfiguration()
                    {
                        RedisOptions = redisconfig,
                        DefaultExpiration = TimeSpan.FromHours(12)
                    };
                }));

            information.RegisterType<ISerializer, JsonSerializer>(Scope.Singleton);

            information.RegisterType<IRedisCache, RedisCache>(Scope.Singleton);

            information.RegisterType<IPnRFoDatabaseService, PnRfoDatabaseService>(Scope.Singleton);
            information.RegisterType<IGatewayDatabaseService, GatewayDatabaseService>(Scope.Singleton);

            information.RegisterType<IDifferentialArchiveService, DifferentialArchiveService>(Scope.Singleton);
            information.RegisterType<IDifferentialDownloadService, DifferentialDownloadService>(Scope.Singleton);

            information.RegisterType<IActiveDirectoryService, ActiveDirectoryService>(Scope.Singleton);
            information.RegisterType<IBatchConfigService, BatchConfigService>(Scope.Singleton);
            information.RegisterType<IScheduleDataService, ScheduleDataService>(Scope.Singleton);
            information.RegisterType<IRedstoneWebRequestScheduler, RedstoneWebRequestScheduler>(Scope.Singleton);
            information.RegisterType<IExecutableScheduler, ExecutableScheduler>(Scope.Singleton);
            information.RegisterType<IBatchConfigDataService, BatchConfigDataService>(Scope.Singleton);
            information.RegisterType<IRiskBatchInterrogationService, RiskBatchInterrogationService>(Scope.Singleton);
            information.RegisterType<IScheduleService<ScheduleBatchModel>, RedstoneBatchScheduleService>(Scope.Singleton);
            information.RegisterType<IScheduleService<ScheduleWebRequestModel>, RedstoneRequestScheduleService>(Scope.Singleton);
            information.RegisterType<IScheduleService<ScheduleExecutableModel>, ExecutableScheduleService>(Scope.Singleton);
            information.RegisterType<IScheduleGroupService, ScheduleGroupService>(Scope.Singleton);
            information.RegisterType<IRequestConfigurationService, RequestConfigurationService>(Scope.Singleton);
            information.RegisterType<IHttpClientProvider, HttpClientProvider>(Scope.Singleton);
            information.RegisterType<IBatchService, BatchService>(Scope.Singleton);
            information.RegisterType<IIssueTrackerService, IssueTrackerService>(Scope.Singleton);

            //Reset Services Registrations
            information.RegisterType<IGatewayService, GatewayService>(Scope.ContainerSingleton);
            information.RegisterType<IBasicRestService, BasicRestService>(Scope.ContainerSingleton);
            information.RegisterType<IServerDiagnosticsService, ServerDiagnosticsService>(Scope.ContainerSingleton);
            information.RegisterType<IDataFeedService, DataFeedService>(Scope.ContainerSingleton);
            information.RegisterType<IBatchHelper, BatchHelper>(Scope.ContainerSingleton);
            information.RegisterType<IDatabaseStateProvider, DatabaseStateProvider>(Scope.ContainerSingleton);
            information.RegisterType<IManifestService, ManifestService>(Scope.ContainerSingleton);
            information.RegisterType<IPdcService, PdcService>(Scope.ContainerSingleton);

            Absa.Cib.Authorization.Extensions.Registration.Register(information);
            Absa.Cib.Authorization.Extensions.Registration.RegisterCertificates(information);
            Absa.Cib.JwtAuthentication.Registrations.Register(information);
            Absa.Cib.JwtAuthentication.Registrations.RegisterCertificates(information);
            Bagl.Cib.MSF.ClientAPI.Registrations.Register(information);
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