// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Abstractions;

namespace Microsoft.Extensions.DependencyInjection.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the specified <paramref name="descriptor"/> to the <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="descriptor">The <see cref="ServiceDescriptor"/>.</param>
        /// <returns>A reference to the current instance of <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection Add(
            this IServiceCollection collection,
            ServiceDescriptor descriptor)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            collection.Add(descriptor);
            return collection;
        }

        /// <summary>
        /// Adds a sequence of <see cref="ServiceDescriptor"/> to the <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="descriptors">The <see cref="IEnumerable{T}"/> of <see cref="ServiceDescriptor"/>s to add.</param>
        /// <returns>A reference to the current instance of <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection Add(
            this IServiceCollection collection,
            IEnumerable<ServiceDescriptor> descriptors)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (descriptors == null)
            {
                throw new ArgumentNullException(nameof(descriptors));
            }

            foreach (var descriptor in descriptors)
            {
                collection.Add(descriptor);
            }

            return collection;
        }

        /// <summary>
        /// Adds the specified <paramref name="descriptor"/> to the <paramref name="collection"/> if the
        /// service type hasn't been already registered.
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="descriptor">The <see cref="ServiceDescriptor"/>.</param>
        public static bool TryAdd(
            this IServiceCollection collection,
            ServiceDescriptor descriptor)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (collection.All(d => d.ServiceType != descriptor.ServiceType))
            {
                collection.Add(descriptor);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds the specified <paramref name="descriptors"/> to the <paramref name="collection"/> if the
        /// service type hasn't been already registered.
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="descriptors">The <see cref="ServiceDescriptor"/>s.</param>
        public static bool TryAdd(
            this IServiceCollection collection,
            IEnumerable<ServiceDescriptor> descriptors)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (descriptors == null)
            {
                throw new ArgumentNullException(nameof(descriptors));
            }

            return descriptors.Any(collection.TryAdd);
        }

        public static bool TryAddTransient(
            this IServiceCollection collection,
            Type service)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            var descriptor = ServiceDescriptor.Transient(service, service);

            return collection.TryAdd(descriptor);
        }

        public static bool TryAddTransient(
            this IServiceCollection collection,
            Type service,
            Type implementationType)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            var descriptor = ServiceDescriptor.Transient(service, implementationType);

            return collection.TryAdd(descriptor);
        }

        public static bool TryAddTransient(
            this IServiceCollection collection,
            Type service,
            Func<IServiceProvider, object> implementationFactory)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationFactory == null)
            {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            var descriptor = ServiceDescriptor.Transient(service, implementationFactory);

            return collection.TryAdd(descriptor);
        }

        public static bool TryAddTransient<TService>(this IServiceCollection collection)
            where TService : class
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            return collection.TryAddTransient(typeof(TService), typeof(TService));
        }

        public static bool TryAddTransient<TService, TImplementation>(this IServiceCollection collection)
            where TService : class
            where TImplementation : class, TService
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            return collection.TryAddTransient(typeof(TService), typeof(TImplementation));
        }

        public static bool TryAddTransient<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            return services.TryAddTransient<TService, TService>(implementationFactory);
        }

        public static bool TryAddTransient<TService, TImplementation>(
            this IServiceCollection services,
            Func<IServiceProvider, TImplementation> implementationFactory)
            where TService : class
            where TImplementation : class, TService
        {
            return services.TryAdd(ServiceDescriptor.Transient(typeof(TService), implementationFactory));
        }

        public static bool TryAddScoped(
            this IServiceCollection collection,
            Type service)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            var descriptor = ServiceDescriptor.Scoped(service, service);

            return collection.TryAdd(descriptor);
        }

        public static bool TryAddScoped(
            this IServiceCollection collection,
            Type service,
            Type implementationType)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            var descriptor = ServiceDescriptor.Scoped(service, implementationType);

            return collection.TryAdd(descriptor);
        }

        public static bool TryAddScoped(
            this IServiceCollection collection,
            Type service,
            Func<IServiceProvider, object> implementationFactory)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationFactory == null)
            {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            var descriptor = ServiceDescriptor.Scoped(service, implementationFactory);

            return collection.TryAdd(descriptor);
        }

        public static bool TryAddScoped<TService>(this IServiceCollection collection)
            where TService : class
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            return collection.TryAddScoped(typeof(TService), typeof(TService));
        }

        public static bool TryAddScoped<TService, TImplementation>(this IServiceCollection collection)
            where TService : class
            where TImplementation : class, TService
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            return collection.TryAddScoped(typeof(TService), typeof(TImplementation));
        }

        public static bool TryAddScoped<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            return services.TryAddScoped<TService, TService>(implementationFactory);
        }

        public static bool TryAddScoped<TService, TImplementation>(
            this IServiceCollection services,
            Func<IServiceProvider, TImplementation> implementationFactory)
            where TService : class
            where TImplementation : class, TService
        {
            return services.TryAdd(ServiceDescriptor.Scoped(typeof(TService), implementationFactory));
        }

        public static bool TryAddSingleton(
            this IServiceCollection collection,
            Type service)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            var descriptor = ServiceDescriptor.Singleton(service, service);

            return collection.TryAdd(descriptor);
        }

        public static bool TryAddSingleton(
            this IServiceCollection collection,
            Type service,
            Type implementationType)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            var descriptor = ServiceDescriptor.Singleton(service, implementationType);

            return collection.TryAdd(descriptor);
        }

        public static bool TryAddSingleton(
            this IServiceCollection collection,
            Type service,
            Func<IServiceProvider, object> implementationFactory)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationFactory == null)
            {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            var descriptor = ServiceDescriptor.Singleton(service, implementationFactory);

            return collection.TryAdd(descriptor);
        }

        public static bool TryAddSingleton<TService>(this IServiceCollection collection)
            where TService : class
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            return collection.TryAddSingleton(typeof(TService), typeof(TService));
        }

        public static bool TryAddSingleton<TService, TImplementation>(this IServiceCollection collection)
            where TService : class
            where TImplementation : class, TService
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            return collection.TryAddSingleton(typeof(TService), typeof(TImplementation));
        }

        public static bool TryAddSingleton<TService>(this IServiceCollection collection, TService instance)
            where TService : class
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var descriptor = ServiceDescriptor.Singleton(typeof(TService), instance);

            return collection.TryAdd(descriptor);
        }

        public static bool TryAddSingleton<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            return services.TryAddSingleton<TService, TService>(implementationFactory);
        }

        public static bool TryAddSingleton<TService, TImplementation>(
            this IServiceCollection services,
            Func<IServiceProvider, TImplementation> implementationFactory)
            where TService : class
            where TImplementation : class, TService
        {
            return services.TryAdd(ServiceDescriptor.Singleton(typeof(TService), implementationFactory));
        }

        /// <summary>
        /// Adds a <see cref="ServiceDescriptor"/> if an existing descriptor with the same
        /// <see cref="ServiceDescriptor.ServiceType"/> and an implementation that does not already exist
        /// in <paramref name="services."/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="descriptor">The <see cref="ServiceDescriptor"/>.</param>
        /// <remarks>
        /// Use <see cref="TryAddEnumerable(IServiceCollection, ServiceDescriptor)"/> when registing a service implementation of a
        /// service type that
        /// supports multiple registrations of the same service type. Using
        /// <see cref="Add(IServiceCollection, ServiceDescriptor)"/> is not idempotent and can add
        /// duplicate
        /// <see cref="ServiceDescriptor"/> instances if called twice. Using
        /// <see cref="TryAddEnumerable(IServiceCollection, ServiceDescriptor)"/> will prevent registration
        /// of multiple implementation types.
        /// </remarks>
        public static bool TryAddEnumerable(
            this IServiceCollection services,
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
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds the specified <see cref="ServiceDescriptor"/>s if an existing descriptor with the same
        /// <see cref="ServiceDescriptor.ServiceType"/> and an implementation that does not already exist
        /// in <paramref name="services."/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="descriptors">The <see cref="ServiceDescriptor"/>s.</param>
        /// <remarks>
        /// Use <see cref="TryAddEnumerable(IServiceCollection, ServiceDescriptor)"/> when registing a service
        /// implementation of a service type that
        /// supports multiple registrations of the same service type. Using
        /// <see cref="Add(IServiceCollection, ServiceDescriptor)"/> is not idempotent and can add
        /// duplicate
        /// <see cref="ServiceDescriptor"/> instances if called twice. Using
        /// <see cref="TryAddEnumerable(IServiceCollection, ServiceDescriptor)"/> will prevent registration
        /// of multiple implementation types.
        /// </remarks>
        public static bool TryAddEnumerable(
            this IServiceCollection services,
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

            return descriptors.Any(services.TryAddEnumerable);
        }

        /// <summary>
        /// Removes the first service in <see cref="IServiceCollection"/> with the same service type
        /// as <paramref name="descriptor"/> and adds <paramef name="descriptor"/> to the collection.
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="descriptor">The <see cref="ServiceDescriptor"/> to replace with.</param>
        /// <returns></returns>
        public static IServiceCollection Replace(
            this IServiceCollection collection,
            ServiceDescriptor descriptor)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            var registeredServiceDescriptor = collection.FirstOrDefault(s => s.ServiceType == descriptor.ServiceType);
            if (registeredServiceDescriptor != null)
            {
                collection.Remove(registeredServiceDescriptor);
            }

            collection.Add(descriptor);
            return collection;
        }
    }
}
