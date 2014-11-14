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

        public ServiceDescriptor Transient<TService, TImplementation>(bool isFallback = false)
        {
            return Describe<TService, TImplementation>(LifecycleKind.Transient, isFallback);
        }

        public ServiceDescriptor Transient(Type service, Type implementationType, bool isFallback = false)
        {
            return Describe(service, implementationType, LifecycleKind.Transient, isFallback);
        }

        public ServiceDescriptor Transient<TService>(Func<IServiceProvider, object> implementationFactory, bool isFallback = false)
        {
            return Describe(typeof(TService), implementationFactory, LifecycleKind.Transient, isFallback);
        }

        public ServiceDescriptor Transient(Type service, Func<IServiceProvider, object> implementationFactory, bool isFallback = false)
        {
            return Describe(service, implementationFactory, LifecycleKind.Transient, isFallback);
        }

        public ServiceDescriptor Scoped<TService, TImplementation>(bool isFallback = false)
        {
            return Describe<TService, TImplementation>(LifecycleKind.Scoped, isFallback);
        }

        public ServiceDescriptor Scoped(Type service, Type implementationType, bool isFallback = false)
        {
            return Describe(service, implementationType, LifecycleKind.Scoped, isFallback);
        }

        public ServiceDescriptor Scoped<TService>(Func<IServiceProvider, object> implementationFactory)
        {
            return Describe(typeof(TService), implementationFactory, LifecycleKind.Scoped);
        }

        public ServiceDescriptor Scoped(Type service, Func<IServiceProvider, object> implementationFactory)
        {
            return Describe(service, implementationFactory, LifecycleKind.Scoped);
        }

        public ServiceDescriptor Singleton<TService, TImplementation>(bool isFallback = false)
        {
            return Describe<TService, TImplementation>(LifecycleKind.Singleton, isFallback);
        }

        public ServiceDescriptor Singleton(Type service, Type implementationType, bool isFallback = false)
        {
            return Describe(service, implementationType, LifecycleKind.Singleton, isFallback);
        }

        public ServiceDescriptor Singleton<TService>(Func<IServiceProvider, object> implementationFactory, bool isFallback = false)
        {
            return Describe(typeof(TService), implementationFactory, LifecycleKind.Singleton, isFallback);
        }

        public ServiceDescriptor Singleton(Type serviceType, Func<IServiceProvider, object> implementationFactory, bool isFallback = false)
        {
            return Describe(serviceType, implementationFactory, LifecycleKind.Singleton, isFallback);
        }

        public ServiceDescriptor Instance<TService>(object implementationInstance, bool isFallback = false)
        {
            return Instance(typeof(TService), implementationInstance, isFallback);
        }

        public ServiceDescriptor Instance(Type serviceType, object implementationInstance, bool isFallback = false)
        {
            var implementationType = GetRemappedImplementationType(serviceType);
            if (implementationType != null)
            {
                return new ServiceDescriptor(serviceType, implementationType, LifecycleKind.Singleton, isFallback);
            }

            return new ServiceDescriptor(serviceType, implementationInstance, isFallback);
        }

        private ServiceDescriptor Describe<TService, TImplementation>(LifecycleKind lifecycle, bool isFallback)
        {
            return Describe(
                typeof(TService),
                typeof(TImplementation),
                lifecycle,
                isFallback);
        }

        public ServiceDescriptor Describe(Type serviceType, Type implementationType, LifecycleKind lifecycle, bool isFallback = false)
        {
            implementationType = GetRemappedImplementationType(serviceType) ?? implementationType;

            return new ServiceDescriptor(serviceType, implementationType, lifecycle, isFallback);
        }

        public ServiceDescriptor Describe(Type serviceType, Func<IServiceProvider, object> implementationFactory, LifecycleKind lifecycle, bool isFallback = false)
        {
            var implementationType = GetRemappedImplementationType(serviceType);
            if (implementationType != null)
            {
                return new ServiceDescriptor(serviceType, implementationType, lifecycle, isFallback);
            }

            return new ServiceDescriptor(serviceType, implementationFactory, lifecycle, isFallback);
        }

        private Type GetRemappedImplementationType(Type serviceType)
        {
            // Allow the user to change the implementation source via configuration.
            var serviceTypeName = serviceType.FullName;
            var implementationTypeName = _configuration.Get(serviceTypeName);
            if (!string.IsNullOrEmpty(implementationTypeName))
            {
                var type = Type.GetType(implementationTypeName, throwOnError: false);
                if (type == null)
                {
                    var message = string.Format("TODO: unable to locate implementation {0} for service {1}", implementationTypeName, serviceTypeName);
                    throw new InvalidOperationException(message);
                }

                return type;
            }

            return null;
        }
    }
}
