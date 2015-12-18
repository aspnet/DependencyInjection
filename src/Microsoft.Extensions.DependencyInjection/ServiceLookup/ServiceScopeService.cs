// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class ServiceScopeService : IService, IServiceCallSite
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
            return new ServiceScopeFactory(provider);
        }

        public Expression Build(Expression provider)
        {
            return Expression.New(
                typeof(ServiceScopeFactory).GetTypeInfo()
                    .DeclaredConstructors
                    .Single(),
                provider);
        }
    }
}
