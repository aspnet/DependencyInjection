// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection.Ordered
{
    public static class ServiceCollectionOrderedExtensions
    {
        public static IServiceCollection AddOrdered<TService>(this IServiceCollection services)
            where TService : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            AddOrdered(services, typeof(TService));
            return services;
        }

        public static IServiceCollection AddOrdered(
            this IServiceCollection services,
            Type serviceType)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            GetOrderedDescriptor(services, serviceType);
            return services;
        }

        public static IServiceCollection AddOrdered<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return AddOrdered(services, (ServiceDescriptor) ServiceDescriptor.Transient(typeof(TService), typeof(TImplementation)));
        }

        public static IServiceCollection AddOrdered<TService>(
            this IServiceCollection services,
            TService implementationInstance)
            where TService : class
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (implementationInstance == null)
            {
                throw new ArgumentNullException(nameof(implementationInstance));
            }

            return AddOrdered(services, (ServiceDescriptor) ServiceDescriptor.Singleton(typeof(TService), implementationInstance));
        }

        public static IServiceCollection AddOrdered(
            this IServiceCollection services,
            Type serviceType,
            object implementationInstance)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationInstance == null)
            {
                throw new ArgumentNullException(nameof(implementationInstance));
            }

            return AddOrdered(services, (ServiceDescriptor) ServiceDescriptor.Singleton(serviceType, implementationInstance));
        }

        public static IServiceCollection AddOrdered(
            this IServiceCollection services,
            Type serviceType,
            Type implementationType)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            return AddOrdered(services, (ServiceDescriptor) ServiceDescriptor.Transient(serviceType, implementationType));
        }

        public static IServiceCollection AddOrdered(
           this IServiceCollection services,
           Type serviceType,
           Func<IServiceProvider, object> implementationFactory)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationFactory == null)
            {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return AddOrdered(services, (ServiceDescriptor) ServiceDescriptor.Transient(serviceType, implementationFactory));
        }

        public static IServiceCollection AddOrdered<TService, TImplementation>(
           this IServiceCollection services,
           Func<IServiceProvider, TImplementation> implementationFactory)
           where TService : class
           where TImplementation : class, TService
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (implementationFactory == null)
            {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return AddOrdered(services, (ServiceDescriptor) ServiceDescriptor.Transient(typeof(TService), implementationFactory));
        }

        public static IServiceCollection AddOrdered(
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

            var collectionDescriptor = GetOrderedDescriptor(collection, descriptor.ServiceType);
            collectionDescriptor.Descriptors.Add(descriptor);
            return collection;
        }

        private static OrderedServiceDescriptor GetOrderedDescriptor(
            this IServiceCollection collection,
            Type serviceType)
        {
            var descriptor = (OrderedServiceDescriptor)
                collection.FirstOrDefault(d =>
                    d.GetType() == typeof(OrderedServiceDescriptor) &&
                    d.ServiceType == serviceType);
            if (descriptor == null)
            {
                descriptor = new OrderedServiceDescriptor(serviceType);
                collection.Add(descriptor);
                var holderType = typeof(OrderedServiceDescriptorHolder<>).MakeGenericType(serviceType);
                collection.AddSingleton(holderType,
                    Activator.CreateInstance(holderType, descriptor));
                collection.TryAddTransient(
                    typeof(IOrdered<>).MakeGenericType(serviceType),
                    typeof(Ordered<>).MakeGenericType(serviceType));
            }
            return descriptor;
        }
    }
}