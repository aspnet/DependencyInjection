// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal abstract class CompiledServiceProviderEngine : ServiceProviderEngine
    {
        public ExpressionResolverBuilder ExpressionResolverBuilder { get; }

        public CompiledServiceProviderEngine(IEnumerable<ServiceDescriptor> serviceDescriptors, IServiceProviderEngineCallback callback, ITrackSingletonServices singletonTracker) : base(serviceDescriptors, callback, singletonTracker)
        {
            ExpressionResolverBuilder = new ExpressionResolverBuilder(RuntimeResolver, this, Root);
        }

        protected override Func<ServiceProviderEngineScope, object> RealizeService(IServiceCallSite callSite)
        {
            var realizedService = ExpressionResolverBuilder.Build(callSite);
            RealizedServices[callSite.ServiceType] = realizedService;
            return realizedService;
        }
    }
}