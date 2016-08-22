using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceEditorExtensions
    {
        public static IServicesEditor<TImplementation> AddTransient<TImplementation>(this IServicesEditor<TImplementation> services)
            where TImplementation : class
        {
            services.Add(ServiceDescriptor.Transient(services.ServiceType, typeof(TImplementation)));
            return services;
        }

        public static IServicesEditor<TImplementation> AddScoped<TImplementation>(this IServicesEditor<TImplementation> services)
            where TImplementation : class
        {
            services.Add(ServiceDescriptor.Scoped(services.ServiceType, typeof(TImplementation)));
            return services;
        }

        public static IServicesEditor<TImplementation> AddSingleton<TImplementation>(this IServicesEditor<TImplementation> services)
            where TImplementation : class
        {
            services.Add(ServiceDescriptor.Singleton(services.ServiceType, typeof(TImplementation)));
            return services;
        }

        public static IServicesEditor<TService> AddSingleton<TService>(
            this IServicesEditor<TService> services,
            TService implementationInstance)
            where TService : class
        {
            services.Add(ServiceDescriptor.Singleton(services.ServiceType, implementationInstance));
            return services;
        }

        public static IServicesEditor AddSingleton(
            this IServicesEditor services,
            object implementationInstance)
        {
            services.Add(ServiceDescriptor.Singleton(services.ServiceType, implementationInstance));
            return services;
        }

        public static IServicesEditor AddTransient(
            this IServicesEditor services,
            Type implementationType)
        {
            services.Add(ServiceDescriptor.Singleton(services.ServiceType, implementationType));
            return services;
        }

        public static IServicesEditor AddTransient(
            this IServicesEditor services,
            Func<IServiceProvider, object> implementationFactory)
        {
            services.Add(ServiceDescriptor.Transient(services.ServiceType, implementationFactory));
            return services;
        }

        public static IServicesEditor<TService> AddTransient<TService, TImplementation>(
            this IServicesEditor<TService> services,
            Func<IServiceProvider, TImplementation> implementationFactory)
            where TService : class
            where TImplementation : class, TService
        {
            services.Add(ServiceDescriptor.Transient(services.ServiceType, implementationFactory));
            return services;
        }
    }
}