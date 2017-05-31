using Bagl.Cib.MIT.IoC;
using Gateway.Web.Database;
using Gateway.Web.Services;

namespace Gateway.Web
{
    public static class Registrations
    {
        public static void Register(ISystemInformation information)
        {
            information.RegisterType<IGatewayDatabaseService, GatewayDatabaseService>(Scope.Singleton);
            information.RegisterType<IGatewayService, GatewayService>(Scope.Singleton);
        }
    }
}