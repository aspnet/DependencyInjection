// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.ConfigurationModel;

namespace Microsoft.Framework.DependencyInjection
{
    public class ServiceDescriber
    {
        private IConfiguration _configuration;

        public ServiceDescriber()
            : this(new Configuration())
        {
        }

        public ServiceDescriber(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ServiceDescriptor Transient<TService, TImplementation>()
        {
            return Describe<TService, TImplementation>(LifecycleKind.Transient);
        }

        public ServiceDescriptor Transient(Type service, Type implementationType)
        {
            return Describe(service, implementationType, LifecycleKind.Transient);
        }

        public ServiceDescriptor Transient<TService>(Func<IServiceProvider, object> implementationFactory)
        {
            return Describe<TService>(implementationFactory, LifecycleKind.Transient);
        }

        public ServiceDescriptor Scoped<TService, TImplementation>()
        {
            return Describe<TService, TImplementation>(LifecycleKind.Scoped);
        }

        public ServiceDescriptor Scoped(Type service, Type implementationType)
        {
            return Describe(service, implementationType, LifecycleKind.Scoped);
        }

        public ServiceDescriptor Scoped<TService>(Func<IServiceProvider, object> implementationFactory)
        {
            return Describe<TService>(implementationFactory, LifecycleKind.Scoped);
        }

        public ServiceDescriptor Singleton<TService, TImplementation>()
        {
            return Describe<TService, TImplementation>(LifecycleKind.Singleton);
        }

        public ServiceDescriptor Singleton(Type service, Type implementationType)
        {
            return Describe(service, implementationType, LifecycleKind.Singleton);
        }

        public ServiceDescriptor Singleton<TService>(Func<IServiceProvider, object> implementationFactory)
        {
            return Describe<TService>(implementationFactory, LifecycleKind.Singleton);
        }

        public ServiceDescriptor Instance<TService>(object implementationInstance)
        {
            return Instance(typeof(TService), implementationInstance);
        }

        public ServiceDescriptor Instance(Type service, object implementationInstance)
        {
            return Describe(
                service,
                implementationType: null,
                implementationInstance: implementationInstance,
                implementationFactory: null,
                lifecycle: LifecycleKind.Singleton);
        }

        private ServiceDescriptor Describe<TService, TImplementation>(LifecycleKind lifecycle)
        {
            return Describe(
                typeof(TService),
                typeof(TImplementation),
                implementationInstance: null,
                implementationFactory: null,
                lifecycle: lifecycle);
        }

        private ServiceDescriptor Describe(
                Type serviceType,
                Type implementationType,
                LifecycleKind lifecycle)
        {
            return Describe(
                serviceType,
                implementationType,
                implementationInstance: null,
                implementationFactory: null,
                lifecycle: lifecycle);
        }

        private ServiceDescriptor Describe<TService>(
                Func<IServiceProvider, object> implementationFactory,
                LifecycleKind lifecycle)
        {
            return Describe(
                typeof(TService),
                implementationType: null,
                implementationInstance: null,
                implementationFactory: implementationFactory,
                lifecycle: lifecycle);
        }

        public ServiceDescriptor Describe(
                Type serviceType,
                Type implementationType,
                object implementationInstance,
                Func<IServiceProvider, object> implementationFactory,
                LifecycleKind lifecycle)
        {
            var serviceTypeName = serviceType.FullName;
            var implementationTypeName = _configuration.Get(serviceTypeName);
            if (!string.IsNullOrEmpty(implementationTypeName))
            {
                try
                {
                    implementationType = Type.GetType(implementationTypeName);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("TODO: unable to locate implementation {0} for service {1}", implementationTypeName, serviceTypeName), ex);
                }
            }

            return new ServiceDescriptor
            {
                ServiceType = serviceType,
                ImplementationType = implementationType,
                ImplementationInstance = implementationInstance,
                ImplementationFactory = implementationFactory,
                Lifecycle = lifecycle
            };
        }
    }
}
