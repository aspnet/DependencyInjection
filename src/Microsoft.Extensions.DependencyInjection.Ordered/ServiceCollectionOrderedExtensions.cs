// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection.Ordered
{
    public static class ServiceCollectionOrderedExtensions
    {
        public static IServicesEditor<TService> AddOrdered<TService>(this IServiceCollection services)
               where TService : class
        {
            return new ServiceEditor<TService>(GetOrderedDescriptor(services, typeof(TService)).Descriptors);
        }

        public static IServicesEditor AddOrdered(this IServiceCollection services, Type serviceType)
        {
            return new ServiceEditor<object>(GetOrderedDescriptor(services, serviceType).Descriptors, serviceType);
        }

        private static OrderedEnumerableServiceDescriptor GetOrderedDescriptor(
            this IServiceCollection collection,
            Type serviceType)
        {
            var descriptor = collection
                .OfType<OrderedEnumerableServiceDescriptor>()
                .FirstOrDefault(d => d.ServiceType == serviceType);

            if (descriptor == null)
            {
                descriptor = new OrderedEnumerableServiceDescriptor(serviceType);
                collection.Add(descriptor);

                var containerType = typeof(OrderedEnumerableServiceDescriptorContainer<>).MakeGenericType(serviceType);
                collection.AddSingleton(containerType,
                    Activator.CreateInstance(containerType, descriptor));

                collection.TryAddTransient(
                    typeof(IOrdered<>).MakeGenericType(serviceType),
                    typeof(Ordered<>).MakeGenericType(serviceType));

                var transientProviderType = typeof(OrderedScopeProvider<>.TransientOrderedScopeProvider).MakeGenericType(serviceType);
                collection.TryAddTransient(transientProviderType, transientProviderType);

                var scopedProviderType = typeof(OrderedScopeProvider<>.ScopedOrderedScopeProvider).MakeGenericType(serviceType);
                collection.TryAddScoped(scopedProviderType, scopedProviderType);

                var singletonProviderType = typeof(OrderedScopeProvider<>.SingletonOrderedScopeProvider).MakeGenericType(serviceType);
                collection.TryAddSingleton(singletonProviderType, singletonProviderType);
            }
            return descriptor;
        }
    }
}