// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;

namespace Microsoft.Framework.DependencyInjection.ServiceLookup
{
    internal class ServiceScopeFactory : IServiceScopeFactory
    {
        private int _capacity = 32 * Environment.ProcessorCount;
        private int _halfCapacity = 32 * Environment.ProcessorCount;
        private readonly ConcurrentQueue<ServiceScope> _scopePool = new ConcurrentQueue<ServiceScope>();

        private readonly ServiceProvider _provider;

        public ServiceScopeFactory(ServiceProvider provider)
        {
            _provider = provider;
        }

        public IServiceScope CreateScope()
        {
            ServiceScope scope;
            // maintain unused buffer of _halfCapacity as partial defense against badly behaving user code
            if (_scopePool.Count > _halfCapacity && _scopePool.TryDequeue(out scope))
            {
                scope.Reset();
                return scope;
            }
            return new ServiceScope(new ServiceProvider(_provider), this);
        }

        internal void PoolScope(ServiceScope scope)
        {
            // Benign race condition
            if (_scopePool.Count < _capacity)
            {
                _scopePool.Enqueue(scope);
            }
        }

    }
}
