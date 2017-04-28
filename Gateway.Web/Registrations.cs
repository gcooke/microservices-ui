using Gateway.Web.Database;
using Gateway.Web.Services;
using Microsoft.Practices.Unity;

namespace Gateway.Web
{
    public static class Registrations
    {
        public static void Register(IUnityContainer container)
        {
            container.RegisterType<IGatewayDatabaseService, GatewayDatabaseService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IGatewayService, GatewayService>(new ContainerControlledLifetimeManager());
        }
    }
}