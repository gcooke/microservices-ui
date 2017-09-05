using System;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MIT.Logging.Impl;
using Bagl.Cib.MSF.ClientAPI.Gateway;
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
            information.RegisterType<IRoleService, RoleService>(Scope.Singleton);
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