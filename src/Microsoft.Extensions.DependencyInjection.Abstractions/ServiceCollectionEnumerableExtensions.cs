// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionEnumerableExtensions
    {
        public static IServicesEditor<TService> AddEnumerable<TService>(this IServiceCollection services)
            where TService : class
        {
            return new ServiceEditor<TService>(GetEnumerableDescriptor(services, typeof(TService)).Descriptors);
        }

        public static IServicesEditor AddEnumerable(this IServiceCollection services, Type serviceType)
        {
            return new ServiceEditor<object>(GetEnumerableDescriptor(services, serviceType).Descriptors);
        }

        private static EnumerableServiceDescriptor GetEnumerableDescriptor(
            this IServiceCollection collection,
            Type serviceType)
        {
            var descriptor = (EnumerableServiceDescriptor)
                collection.FirstOrDefault(d =>
                    d.GetType() == typeof(EnumerableServiceDescriptor) &&
                    d.ServiceType == serviceType);
            if (descriptor == null)
            {
                descriptor = new EnumerableServiceDescriptor(serviceType);
                collection.Add(descriptor);
            }
            return descriptor;
        }
    }
}