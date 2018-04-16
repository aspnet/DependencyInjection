// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    /// <summary>
    /// Summary description for IServiceCallSite
    /// </summary>
    internal abstract class ServiceCallSite
    {
        public ServiceCallSite(ResultCache cache)
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
        Dispose,
        None
    }

    internal struct ServiceCacheKey: IEquatable<ServiceCacheKey>
    {
        public Type Type { get; }
        public int Slot { get; }
        public static ServiceCacheKey Enpty { get; } = new ServiceCacheKey(null, 0);

        public ServiceCacheKey(Type type, int slot)
        {
            Type = type;
            Slot = slot;
        }

        public bool Equals(ServiceCacheKey other)
        {
            return Type == other.Type && Slot == other.Slot;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Type.GetHashCode() * 397) ^ Slot;
            }
        }
    }

    internal struct ResultCache
    {
        public static ResultCache None { get; } = new ResultCache(CallSiteResultCacheLocation.None, ServiceCacheKey.Enpty);

        public ResultCache(CallSiteResultCacheLocation lifetime, ServiceCacheKey cacheKey)
        {
            Location = lifetime;
            Key = cacheKey;
        }

        public ResultCache(ServiceLifetime lifetime, Type type, int slot)
        {
            Debug.Assert(lifetime == ServiceLifetime.Transient || type != null);

            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    Location = CallSiteResultCacheLocation.Root;
                    break;
                case ServiceLifetime.Scoped:
                    Location = CallSiteResultCacheLocation.Scope;
                    break;
                case ServiceLifetime.Transient:
                    Location = CallSiteResultCacheLocation.Dispose;
                    break;
                default:
                    Location = CallSiteResultCacheLocation.None;
                    break;
            }
            Key = new ServiceCacheKey(type, slot);
        }

        public CallSiteResultCacheLocation Location { get; set; }
        public ServiceCacheKey Key { get; set; }
    }
}