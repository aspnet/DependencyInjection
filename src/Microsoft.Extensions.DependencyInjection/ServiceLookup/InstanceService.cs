// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    /// <summary>
    /// Summary description for InstanceService
    /// </summary>
    internal class InstanceService : IService, IServiceCallSite
    {
        private readonly ServiceDescriptor _descriptor;

        public InstanceService(ServiceDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public IService Previous { get; set; }

        public IService Next { get; set; }

        public ServiceLifetime Lifetime => _descriptor.Lifetime;

        public IServiceCallSite CreateCallSite(ServiceProvider provider)
        {
            return this;
        }

        public object Invoke(ServiceProvider provider)
        {
            return _descriptor.ImplementationInstance;
        }

        public Expression Build(Expression provider)
        {
            return Expression.Constant(_descriptor.ImplementationInstance, _descriptor.ServiceType);
        }
    }
}
