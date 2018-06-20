using System;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.IO;
using Bagl.Cib.MIT.IO.Impl;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MIT.Logging.Impl;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Bagl.Cib.MSF.ClientAPI.Provider;
using Gateway.Web.Database;
using Gateway.Web.Services;

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
            information.RegisterType<IAuthenticationProvider, AuthenticationProvider>(Scope.Singleton);
            information.RegisterType<IDateTimeProvider, DateTimeProvider>(Scope.Singleton);
            information.RegisterType<IRoleService, RoleService>(Scope.Singleton);
            information.RegisterType<IFileService, FileService>(Scope.Singleton);
            information.RegisterType<IDifferentialArchiveService, DifferentialArchiveService>(Scope.Singleton);
            information.RegisterType<IDifferentialDownloadService, DifferentialDownloadService>(Scope.Singleton);
            information.RegisterType<IAuthenticationProvider, AuthenticationProvider>(Scope.Singleton);
            information.RegisterType<IDateTimeProvider, DateTimeProvider>(Scope.Singleton);
            information.RegisterType<ILogsService, LogsService>(Scope.Singleton);
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