using System;
using System.Web;
using Unity;

namespace Gateway.Web.Authorization
{
    public static class HttpContainerProvider
    {
        private static IUnityContainer _container;

        public static IUnityContainer Container
        {
            get
            {
                if (_container != null)
                    return _container;

                if (HttpContext.Current == null)
                    throw new InvalidOperationException("HttpContext not available");

                if (HttpContext.Current.Items[MvcApplication.ContainerKey] == null)
                    throw new InvalidOperationException("IoC Container not found.");

                return (IUnityContainer)HttpContext.Current.Items[MvcApplication.ContainerKey];
            }
        }


        public static void SetContainer(IUnityContainer container)
        {
            _container = container;
        }
    }
}