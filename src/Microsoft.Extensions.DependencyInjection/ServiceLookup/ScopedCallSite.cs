// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    class ScopedCallSite : IServiceCallSite
    {
        internal IService Key { get; }
        internal IServiceCallSite ServiceCallSite { get; }

        public ScopedCallSite(IService key, IServiceCallSite serviceCallSite)
        {
            Key = key;
            ServiceCallSite = serviceCallSite;
        }

        public virtual object Invoke(ServiceProvider provider)
        {
            object resolved;
            lock (provider._resolvedServices)
            {
                if (!provider._resolvedServices.TryGetValue(Key, out resolved))
                {
                    resolved = ServiceCallSite.Invoke(provider);
                    provider._resolvedServices.Add(Key, resolved);
                }
            }
            return resolved;
        }
    }
}