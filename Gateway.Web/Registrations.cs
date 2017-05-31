using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MIT.Logging.Impl;
using Gateway.Web.Database;
using Gateway.Web.Services;

namespace Gateway.Web
{
    public static class Registrations
    {
        public static void Register(ISystemInformation information)
        {
            information.RegisterType<ILogFileService, LogFileService>(Scope.Singleton);
            information.RegisterType<ILoggingService, DefaultLoggingService>(Scope.Singleton);
            information.RegisterType<IGatewayDatabaseService, GatewayDatabaseService>(Scope.Singleton);
            information.RegisterType<IGatewayService, GatewayService>(Scope.Singleton);
        }
    }
}