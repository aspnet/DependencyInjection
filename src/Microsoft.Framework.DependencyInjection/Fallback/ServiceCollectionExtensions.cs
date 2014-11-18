// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.DependencyInjection.ServiceLookup;

namespace Microsoft.Framework.DependencyInjection.Fallback
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceProvider BuildServiceProvider(this IEnumerable<IServiceDescriptor> collection)
        {
            return new ServiceProvider(collection);
        }

        public static IServiceProvider BuildFallbackServiceProvider(this IEnumerable<IServiceDescriptor> collection)
        {
            // Build the manifest
            var manifestTypes = collection.Where(t => t.ServiceType.GenericTypeArguments.Length == 0 && t.ServiceType != typeof(IServiceManifest))
                .Select(t => t.ServiceType);
            return new ServiceProvider(collection.Concat(new IServiceDescriptor[]
            {
                new ServiceDescriber().Instance<IServiceManifest>(new ServiceManifest(manifestTypes))
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
