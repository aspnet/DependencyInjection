// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class ServiceScopeFactory : IServiceScopeFactory
    {
        private readonly ServiceProviderEngineScope _provider;

        public ServiceScopeFactory(ServiceProviderEngineScope provider)
        {
            _provider = provider;
        }

        public IServiceScope CreateScope()
        {
            return new ServiceProviderEngineScope(_provider);
        }
    }
}
