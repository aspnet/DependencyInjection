// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Framework.DependencyInjection
{
    public static class IEnumerableServiceDescriptorExtensions
    {
        private static bool IsDefinedInFallback(IServiceProvider fallback, Type type)
        {
            if (fallback == null)
            {
                return false;
            }
            var scopeFactory = fallback.GetService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                return scope.ServiceProvider.GetService(type) != null;
            }
        }

        public static IEnumerable<IServiceDescriptor> RemoveDuplicateFallbackServices(this IEnumerable<IServiceDescriptor> descriptors, IServiceProvider fallback)
        {
            var results = new List<IServiceDescriptor>();
            // For each service type
            foreach (var group in descriptors.GroupBy(d => d.ServiceType))
            {
                var serviceType = group.Key;

                // Add all non fallback services
                bool lookForFallback = true;
                foreach (var descriptor in group.Where(d => !d.IsFallback))
                {
                    lookForFallback = false;
                    results.Add(descriptor);
                }
                // If no non fallback services were added, 
                // add the fallback only if the parent service provider has not defined
                if (lookForFallback && !IsDefinedInFallback(fallback, serviceType))
                {
                    var lastFallback = group.Where(d => d.IsFallback).LastOrDefault();
                    if (lastFallback != null)
                    {
                        results.Add(lastFallback);
                    }
                }
            }
            return results;
        }
    }
}
