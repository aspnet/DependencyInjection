// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class ServiceProviderEngine : IServiceProviderEngine, IServiceScopeFactory
    {
        private readonly IServiceProviderEngineCallback _callback;

        // CallSiteRuntimeResolver is stateless so can be shared between all instances
        private readonly CallSiteRuntimeResolver _callSiteRuntimeResolver = new CallSiteRuntimeResolver();

        private readonly CallSiteExpressionBuilder ExpressionBuilder;

        private readonly Func<Type, Func<ServiceProviderEngineScope, object>> _createServiceAccessor;

        private readonly ServiceProviderMode _mode;

        public ServiceProviderEngine(IEnumerable<ServiceDescriptor> serviceDescriptors, ServiceProviderOptions options, IServiceProviderEngineCallback callback)
        {
            _callback = callback;
            _createServiceAccessor = CreateServiceAccessor;
            _mode = options.Mode;

            Root = new ServiceProviderEngineScope(this);
            CallSiteFactory = new CallSiteFactory(serviceDescriptors);
            ExpressionBuilder = new CallSiteExpressionBuilder(_callSiteRuntimeResolver, this, Root);
            CallSiteFactory.Add(typeof(IServiceProvider), new ServiceProviderCallSite());
            CallSiteFactory.Add(typeof(IServiceScopeFactory), new ServiceScopeFactoryCallSite());
        }

        internal ConcurrentDictionary<Type, Func<ServiceProviderEngineScope, object>> RealizedServices { get; } =
            new ConcurrentDictionary<Type, Func<ServiceProviderEngineScope, object>>();

        internal CallSiteFactory CallSiteFactory { get; }

        public ServiceProviderEngineScope Root { get; }
        public IServiceScope RootScope => Root;

        public object GetService(Type serviceType) => GetService(serviceType, Root);

        public void Dispose() => Root.Dispose();

        private Func<ServiceProviderEngineScope, object> CreateServiceAccessor(Type serviceType)
        {
            var callSite = CallSiteFactory.CreateCallSite(serviceType, new HashSet<Type>());
            if (callSite != null)
            {
                _callback?.OnCreate(serviceType, callSite);
                return RealizeService(serviceType, callSite);
            }

            return _ => null;
        }

        internal Func<ServiceProviderEngineScope, object> RealizeService(Type serviceType, IServiceCallSite callSite)
        {
            var callCount = 0;
            return scope =>
            {
                Func<ServiceProviderEngineScope, object> CompileResolver()
                {
                    var realizedService = ExpressionBuilder.Build(callSite);
                    RealizedServices[serviceType] = realizedService;
                    return realizedService;
                }

                switch (_mode)
                {
                    case ServiceProviderMode.Dynamic:
                        if (Interlocked.Increment(ref callCount) == 2)
                        {
                            Task.Run(() => CompileResolver());
                        }
                        return _callSiteRuntimeResolver.Resolve(callSite, scope);
                    case ServiceProviderMode.Compiled:
                        return CompileResolver()(scope);
                    case ServiceProviderMode.Runtime:
                        return (RealizedServices[serviceType] = p => _callSiteRuntimeResolver.Resolve(callSite, p))(scope);
                    default:
                        throw new ArgumentException();
                }
            };
        }

        internal object GetService(Type serviceType, ServiceProviderEngineScope serviceProviderEngineScope)
        {
            var realizedService = RealizedServices.GetOrAdd(serviceType, _createServiceAccessor);
            _callback?.OnResolve(serviceType, serviceProviderEngineScope);
            return realizedService.Invoke(serviceProviderEngineScope);
        }

        public IServiceScope CreateScope()
        {
            return new ServiceProviderEngineScope(this);
        }
    }
}