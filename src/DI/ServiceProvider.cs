// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection.ServiceLookup;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// The default IServiceProvider.
    /// </summary>
    public sealed class ServiceProvider : IServiceProvider, IDisposable, IServiceProviderEngineCallback
    {
        private readonly IServiceProviderEngine _engine;

        private readonly CallSiteValidator _callSiteValidator;

        internal ServiceProvider(IEnumerable<ServiceDescriptor> serviceDescriptors, ServiceProviderOptions options)
        {
            IServiceProviderEngineCallback callback = null;
            if (options.ValidateScopes)
            {
                callback = this;
                _callSiteValidator = new CallSiteValidator();
            }
            _engine = new ServiceProviderEngine(serviceDescriptors, options, callback);
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object GetService(Type serviceType) => _engine.GetService(serviceType);

        /// <inheritdoc />
        public void Dispose() => _engine.Dispose();

        void IServiceProviderEngineCallback.OnCreate(Type serviceType, IServiceCallSite callSite)
        {
            _callSiteValidator.ValidateCallSite(serviceType, callSite);
        }

        void IServiceProviderEngineCallback.OnResolve(Type serviceType, IServiceScope scope)
        {
            _callSiteValidator.ValidateResolution(serviceType, scope, _engine.RootScope);
        }
    }
}
