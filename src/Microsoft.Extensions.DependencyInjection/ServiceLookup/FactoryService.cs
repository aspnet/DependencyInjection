// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class FactoryService : IServiceCallSite
    {
        public Func<IServiceProvider, object> Factory { get; }

        public FactoryService(Type serviceType, Func<IServiceProvider, object> factory)
        {
            Factory = factory;
            ServiceType = serviceType;
        }

        public Type ServiceType { get; }
        public Type ImplementationType { get; } = typeof(object);
    }
}
