// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.DependencyInjection.ServiceLookup;

namespace Microsoft.Framework.DependencyInjection.Fallback
{
    public static class IEnumerableServiceDescriptorExtensions
    {
        public static IServiceProvider BuildServiceProvider(this IEnumerable<IServiceDescriptor> services)
        {
            return new ServiceProvider(services);
        }

        public static IServiceProvider BuildFallbackServiceProvider(this IEnumerable<IServiceDescriptor> services)
        {
            // Build the manifest
            var manifestTypes = services.Where(t => t.ServiceType.GetTypeInfo().GenericTypeParameters.Length == 0
                    && t.ServiceType != typeof(IServiceManifest)
                    && t.ServiceType != typeof(IServiceProvider))
                    .Select(t => t.ServiceType).Distinct();
            return new ServiceProvider(services.Concat(new IServiceDescriptor[]
            {
                new ServiceDescriber()
                    .Instance<IServiceManifest>(
                        new ServiceManifest(manifestTypes))
            }));
        }

        private class ServiceManifest : IServiceManifest
        {
            public ServiceManifest(IEnumerable<Type> services)
            {
                Services = services;
            }

            public IEnumerable<Type> Services { get; private set; }
        }
    }
}
