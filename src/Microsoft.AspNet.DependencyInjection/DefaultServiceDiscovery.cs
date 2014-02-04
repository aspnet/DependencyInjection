using System;
using System.Linq;

namespace Microsoft.AspNet.DependencyInjection
{
    internal static class DefaultServiceDiscovery
    {
        public static void AddServices(ServiceProvider services)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var attribute in assembly.GetCustomAttributes(typeof(DefaultServiceAttribute), inherit: false).Cast<DefaultServiceAttribute>())
                {
                    services.Add(attribute.ServiceType, attribute.ImplementationType);
                }
            }
        }
    }
}
