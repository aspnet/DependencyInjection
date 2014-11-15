// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Framework.DependencyInjection.Fallback
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceProvider BuildServiceProvider(
                this IEnumerable<IServiceDescriptor> collection)
        {
            return BuildServiceProvider(collection, fallbackServices: null);
        }

        public static IServiceProvider BuildServiceProvider(
                this IEnumerable<IServiceDescriptor> collection,
                IServiceProvider fallbackServices)
        {
            return new ServiceProvider(collection, fallbackServices);
        }

        // TODO: How to name overload that generates a manifest
        public static IServiceProvider BuildFallbackServiceProvider(
                this IEnumerable<IServiceDescriptor> collection)
        {
            return new ServiceProvider(collection, null, generateManifest:true);
        }

    }
}
