using System;
using System.Linq;
using System.Collections;
using System.Reflection;

namespace Microsoft.AspNet.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSetup(this IServiceCollection services, Type setupType)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }

            var serviceTypes = setupType.GetInterfaces()
                .Where(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IOptionsSetup<>));
            foreach (var serviceType in serviceTypes)
            {
                services.Add(new ServiceDescriptor
                {
                    ServiceType = serviceType,
                    ImplementationType = setupType,
                    Lifecycle = LifecycleKind.Transient
                });
            }
            return services;
        }

        public static IServiceCollection AddSetup<TSetup>(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }

            return services.AddSetup(typeof(TSetup));
        }

        public static IServiceCollection AddSetup(this IServiceCollection services, object setupInstance)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }
            if (setupInstance == null)
            {
                throw new ArgumentNullException("setupInstance");
            }

            var setupType = setupInstance.GetType();
            var serviceTypes = setupType.GetInterfaces()
                .Where(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IOptionsSetup<>));
            foreach (var serviceType in serviceTypes)
            {
                services.Add(new ServiceDescriptor
                {
                    ServiceType = serviceType,
                    ImplementationInstance = setupInstance,
                    Lifecycle = LifecycleKind.Singleton
                });
            }
            return services;
        }

    }
}
