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
            information.RegisterType<ILogFileService, LogFileService>(Scope.Singleton);
            information.RegisterType<ILoggingService, DefaultLoggingService>(Scope.Singleton);
            information.RegisterType<IGatewayDatabaseService, GatewayDatabaseService>(Scope.Singleton);
            information.RegisterType<IGatewayService, GatewayService>(Scope.Singleton);
            information.RegisterType<IRestService, RestService>(Scope.Singleton);
            information.RegisterType<IGatewayRestService, GatewayRestService>(Scope.Singleton);
            information.RegisterType<IRoleService, RoleService>(Scope.Singleton);
        }
    }
}