using Gateway.Web.Database;
using Microsoft.Practices.Unity;

namespace Gateway.Web
{
    public static class Registrations
    {
        public static void Register(IUnityContainer container)
        {
            container.RegisterType<IGatewayDataService, GatewayDataService>(new ContainerControlledLifetimeManager());
        }
    }
}