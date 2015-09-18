// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Framework.DependencyInjection.ServiceLookup
{
    internal class ServiceScope : IServiceScope
    {
        private readonly ServiceProvider _scopedProvider;
        private readonly ServiceScopeFactory _parentFactory;
        
        public ServiceScope(ServiceProvider scopedProvider, ServiceScopeFactory parentFactory)
        {
            _scopedProvider = scopedProvider;
            _parentFactory = parentFactory;
        }

        public IServiceProvider ServiceProvider
        {
            get { return _scopedProvider; }
        }

        internal void Reset()
        {
            _scopedProvider.Reset();
        }

        public void Dispose()
        {
            if (_scopedProvider.Disposed)
            {
                return;
            }

            _scopedProvider.Dispose();

            if (_parentFactory != null)
            {
                _parentFactory.PoolScope(this);
            }
        }
    }
}
