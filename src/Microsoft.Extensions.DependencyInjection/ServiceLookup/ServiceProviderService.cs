// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class ServiceProviderService : IService, IServiceCallSite
    {
        public IService Previous { get; set; }

        public IService Next { get; set; }

        public ServiceLifetime Lifetime => ServiceLifetime.Scoped;

        public IServiceCallSite CreateCallSite(ServiceProvider provider)
        {
            return this;
        }

        public object Invoke(ServiceProvider provider)
        {
            return provider;
        }

        public Expression Build(Expression provider)
        {
            return provider;
        }
    }
}
