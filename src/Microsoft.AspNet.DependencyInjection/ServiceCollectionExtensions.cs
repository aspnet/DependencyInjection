using System;
using System.Linq;
using System.Collections;
using System.Reflection;

namespace Microsoft.AspNet.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }
            return services.AddTransient(typeof(TService), typeof(TImplementation));
        }

        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }
            return services.AddTransient(serviceType, serviceType);
        }

        public static IServiceCollection AddTransient<TService>(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }
            return services.AddTransient(typeof(TService));
        }

        public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }
            return services.AddScoped(typeof(TService), typeof(TImplementation));
        }

        public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }
            return services.AddScoped(serviceType, serviceType);
        }

        public static IServiceCollection AddScoped<TService>(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }
            return services.AddScoped(typeof(TService));
        }

        public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }
            return services.AddSingleton(typeof(TService), typeof(TImplementation));
        }

        public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }
            return services.AddSingleton(serviceType, serviceType);
        }

        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }
            return services.AddSingleton(typeof(TService));
        }

        public static IServiceCollection AddInstance<TService>(this IServiceCollection services, TService implementationInstance)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }
            return services.AddInstance(typeof(TService), implementationInstance);
        }

        public static IServiceCollection AddSetup(this IServiceCollection services, Type setupType)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }

            var serviceTypes = setupType.GetInterfaces()
                .Where(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof (IOptionsSetup<>));
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

            return services.AddSetup(typeof (TSetup));
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
                .Where(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof (IOptionsSetup<>));
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


        public static IServiceCollection SetupOptions<TOptions>(this IServiceCollection services,
            Action<TOptions> setupAction,
            int order)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }
            services.AddSetup(new OptionsSetup<TOptions>(setupAction) {Order = order});
            return services;
        }

        public static IServiceCollection SetupOptions<TOptions>(this IServiceCollection services,
            Action<TOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }
            return services.SetupOptions<TOptions>(setupAction, order: 0);
        }
    }
}
