// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    /// <summary>
    /// Summary description for IServiceCallSite
    /// </summary>
    internal abstract class IServiceCallSite
    {
        public IServiceCallSite(ResultCache cache)
        {
            Cache = cache;
        }

        public abstract Type ServiceType { get; }
        public abstract Type ImplementationType { get; }
        public abstract CallSiteKind Kind { get; }
        public ResultCache Cache { get; }
    }

    internal enum CallSiteResultCacheLocation
    {
        Root,
        Scope,
        None
    }

    internal struct ResultCache
    {
        public ResultCache(CallSiteResultCacheLocation lifetime, object cacheKey)
        {
            Location = lifetime;
            Key = cacheKey;
        }


        public ResultCache(ServiceLifetime lifetime, object cacheKey)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    Location = CallSiteResultCacheLocation.Root;
                    break;
                case ServiceLifetime.Scoped:
                    Location = CallSiteResultCacheLocation.Root;
                    break;
                default:
                    Location = CallSiteResultCacheLocation.None;
                    break;
            }
            Key = cacheKey;
        }

        public CallSiteResultCacheLocation Location { get; set; }
        public object Key { get; set; }
    }
}