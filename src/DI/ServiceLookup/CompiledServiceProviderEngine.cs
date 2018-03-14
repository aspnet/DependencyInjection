// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal abstract class CompiledServiceProviderEngine : ServiceProviderEngine
    {
        public IResolverBuilder ExpressionResolverBuilder { get; }

        public CompiledServiceProviderEngine(IEnumerable<ServiceDescriptor> serviceDescriptors, IServiceProviderEngineCallback callback) : base(serviceDescriptors, callback)
        {
#if NET461 || NETCOREAPP2_1
            ExpressionResolverBuilder = new ILEmitResolverBuilder(RuntimeResolver, this, Root);
#else
            ExpressionResolverBuilder = new ExpressionResolverBuilder(RuntimeResolver, this, Root);
#endif
        }

        protected override Func<ServiceProviderEngineScope, object> RealizeService(IServiceCallSite callSite)
        {
            var realizedService = ExpressionResolverBuilder.Build(callSite);
            RealizedServices[callSite.ServiceType] = realizedService;
            return realizedService;
        }
    }
}