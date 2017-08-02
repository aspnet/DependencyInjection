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
    public sealed class ServiceProvider : IServiceProvider, IDisposable
    {
        private IServiceProviderEngine _engine;

        internal ServiceProvider(IEnumerable<ServiceDescriptor> serviceDescriptors, ServiceProviderOptions options)
        {
            _engine = new ServiceProviderEngine(serviceDescriptors, options);

            if (options.ValidateScopes)
            {
                var callSiteValidator = new CallSiteValidator();
                _engine.OnCreate += (serviceType, callSite) => callSiteValidator.ValidateCallSite(serviceType, callSite);
                _engine.OnResolve += (serviceType, scope) => callSiteValidator.ValidateResolution(serviceType, scope, _engine.RootScope);
            }
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object GetService(Type serviceType) => _engine.GetService(serviceType);

        /// <inheritdoc />
        public void Dispose() => _engine.Dispose();
    }
}
