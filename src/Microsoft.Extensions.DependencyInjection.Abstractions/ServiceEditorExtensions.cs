using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;

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


        /// <summary>
        /// Adds a <see cref="ServiceDescriptor"/> if an existing descriptor with the same
        /// <see cref="ServiceDescriptor.ServiceType"/> and an implementation that does not already exist
        /// in <paramref name="services."/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="descriptor">The <see cref="ServiceDescriptor"/>.</param>
        /// <remarks>
        /// Use <see cref="TryAddImplementation(IServicesEditor, ServiceDescriptor)"/> when registing a service implementation of a
        /// service type that
        /// supports multiple registrations of the same service type. Using
        /// <see cref="ServiceCollectionDescriptorExtensions.Add(IServiceCollection, ServiceDescriptor)"/> is not idempotent and can add
        /// duplicate
        /// <see cref="ServiceDescriptor"/> instances if called twice. Using
        /// <see cref="TryAddImplementation(IServicesEditor, ServiceDescriptor)"/> will prevent registration
        /// of multiple implementation types.
        /// </remarks>
        public static void TryAddImplementation(
            this IServicesEditor services,
            ServiceDescriptor descriptor)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            var implementationType = descriptor.GetImplementationType();

            if (implementationType == typeof(object) ||
                implementationType == descriptor.ServiceType)
            {
                throw new ArgumentException(
                    Resources.FormatTryAddIndistinguishableTypeToEnumerable(
                        implementationType,
                        descriptor.ServiceType),
                    nameof(descriptor));
            }

            if (!services.Any(d =>
                              d.ServiceType == descriptor.ServiceType &&
                              d.GetImplementationType() == implementationType))
            {
                services.Add(descriptor);
            }
        }

        /// <summary>
        /// Adds the specified <see cref="ServiceDescriptor"/>s if an existing descriptor with the same
        /// <see cref="ServiceDescriptor.ServiceType"/> and an implementation that does not already exist
        /// in <paramref name="services."/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="descriptors">The <see cref="ServiceDescriptor"/>s.</param>
        /// <remarks>
        /// Use <see cref="TryAddImplementation(IServicesEditor, ServiceDescriptor)"/> when registing a service
        /// implementation of a service type that
        /// supports multiple registrations of the same service type. Using
        /// <see cref="ServiceCollectionDescriptorExtensions.Add(IServiceCollection, ServiceDescriptor)"/> is not idempotent and can add
        /// duplicate
        /// <see cref="ServiceDescriptor"/> instances if called twice. Using
        /// <see cref="TryAddImplementation(IServicesEditor, ServiceDescriptor)"/> will prevent registration
        /// of multiple implementation types.
        /// </remarks>
        public static void TryAddImplementation(
            this IServicesEditor services,
            IEnumerable<ServiceDescriptor> descriptors)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (descriptors == null)
            {
                throw new ArgumentNullException(nameof(descriptors));
            }

            foreach (var d in descriptors)
            {
                services.TryAddImplementation(d);
            }
        }

        internal static Type GetImplementationType(this ServiceDescriptor descriptor)
        {
            var factoryServiceDescriptor = descriptor as FactoryServiceDescriptor;
            if (factoryServiceDescriptor != null)
            {
                var typeArguments = factoryServiceDescriptor.ImplementationFactory.GetType().GenericTypeArguments;

                Debug.Assert(typeArguments.Length == 2);

                return typeArguments[1];
            }

            var typeServiceDescriptor = descriptor as TypeServiceDescriptor;
            if (typeServiceDescriptor != null)
            {
                return typeServiceDescriptor.ImplementationType;
            }

            var instanceServiceDescriptor = descriptor as InstanceServiceDescriptor;
            if (instanceServiceDescriptor != null)
            {
                return instanceServiceDescriptor.ImplementationInstance.GetType();
            }

            throw new NotSupportedException($"Unsupported service descriptor type '{descriptor.GetType()}'");
        }
    }
}