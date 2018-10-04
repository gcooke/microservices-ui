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
using Gateway.Web.Models.Schedule;
using Gateway.Web.Models.Schedule.Input;
using Gateway.Web.Services;
using Gateway.Web.Services.Batches;
using Gateway.Web.Services.Batches.Interfaces;
using StackExchange.Redis;
using Gateway.Web.Services.Monitoring.RiskReports;
using Gateway.Web.Services.Monitoring.ServerDiagnostics;
using Gateway.Web.Services.Schedule;
using Gateway.Web.Services.Schedule.Interfaces;

namespace Gateway.Web
{
    public static class Registrations
    {
        public static void Register(ISystemInformation information)
        {
            SetupLogging(information);
            information.RegisterType<ILogFileService, LogFileService>(Scope.Singleton);
            information.RegisterType<ILoggingService, DefaultLoggingService>(Scope.Singleton);
            information.RegisterType<IGatewayDatabaseService, GatewayDatabaseService>(Scope.Singleton);
            information.RegisterType<IGatewayService, GatewayService>(Scope.Singleton);
            information.RegisterType<IRestService, RestService>(Scope.Singleton);
            information.RegisterType<IGatewayRestService, GatewayRestService>(Scope.Singleton);
            information.RegisterType<IBasicRestService, BasicRestService>(Scope.Singleton);
            information.RegisterType<IRoleService, RoleService>(Scope.Singleton);
            information.RegisterType<IAuthenticationProvider, AuthenticationProvider>(Scope.ContainerSingleton);
            information.RegisterType<IImpersonationProvider, ImpersonationProvider>(Scope.ContainerSingleton);

            information.RegisterType<IDateTimeProvider, DateTimeProvider>(Scope.Singleton);
            information.RegisterType<IRoleService, RoleService>(Scope.Singleton);
            information.RegisterType<IFileService, FileService>(Scope.Singleton);
            information.RegisterType<IDifferentialArchiveService, DifferentialArchiveService>(Scope.Singleton);
            information.RegisterType<IDifferentialDownloadService, DifferentialDownloadService>(Scope.Singleton);
            information.RegisterType<IDateTimeProvider, DateTimeProvider>(Scope.Singleton);
            information.RegisterType<IRiskReportMonitoringService, RiskReportMonitoringService>(Scope.Singleton);
            information.RegisterType<IGateway, Bagl.Cib.MSF.ClientAPI.Gateway.Gateway>(Scope.Singleton);
            information.RegisterType<ISimpleRestService, SimpleRestService>(Scope.Singleton);
            information.RegisterType<ILogsService, LogsService>(Scope.Singleton);
            information.RegisterType<IRedisConnectionProvider, RedisConnectionProvider>(Scope.Singleton);
            information.RegisterType<IActiveDirectoryService, ActiveDirectoryService>(Scope.Singleton);
            information.RegisterType<IServerDiagnosticsService, ServerDiagnosticsService>(Scope.Singleton);
            information.RegisterType<IBatchConfigService, BatchConfigService>(Scope.Singleton);
            information.RegisterType<IScheduleDataService, ScheduleDataService>(Scope.Singleton);
            information.RegisterType<IRedstoneWebRequestScheduler, RedstoneWebRequestScheduler>(Scope.Singleton);
            information.RegisterType<IBatchConfigDataService, BatchConfigDataService>(Scope.Singleton);
            information.RegisterType<IScheduleService<ScheduleBatchModel>, RedstoneBatchScheduleService>(Scope.Singleton);
            information.RegisterType<IScheduleService<ScheduleWebRequestModel>, RedstoneRequestScheduleService>(Scope.Singleton);
            information.RegisterType<IScheduleGroupService, ScheduleGroupService>(Scope.Singleton);
            information.RegisterType<IRequestConfigurationService, RequestConfigurationService>(Scope.Singleton);
            information.RegisterType<IParentRequestDetailProvider, ParentRequestDetailProvider>(Scope.Singleton);
            RegisterRedisOptions(information);
        }

        private static void RegisterRedisOptions(ISystemInformation information)
        {
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
        }

        private static void SetupLogging(ISystemInformation information)
        {
            information.LoggingInformation.LogPath = information.GetSetting("LogLocation");
            information.LoggingInformation.FileLogLevel.SetLogLevel(information.GetSetting("ConsoleLogLevel"));
            information.LoggingInformation.FileLogLevel.SetLogLevel(information.GetSetting("FileLogLevel"));
            information.LoggingInformation.ImportantLogLevel.SetLogLevel(information.GetSetting("ImportantLogLevel"));
        }

        private static void SetLogLevel(this LogLevel value, string logLevel)
        {
            LogLevel level;
            value = (Enum.TryParse(logLevel, out level)) ? level : value;
        }
    }
}