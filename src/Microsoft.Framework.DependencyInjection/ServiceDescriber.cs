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

        public ServiceDescriptor Transient<TService, TImplementation>(OverrideMode mode = OverrideMode.OverrideMany)
        {
            return Describe<TService, TImplementation>(LifecycleKind.Transient, mode);
        }

        public ServiceDescriptor Transient(Type service, Type implementationType, OverrideMode mode = OverrideMode.OverrideMany)
        {
            return Describe(service, implementationType, LifecycleKind.Transient, mode);
        }

        public ServiceDescriptor Transient<TService>(Func<IServiceProvider, object> implementationFactory, OverrideMode mode = OverrideMode.OverrideMany)
        {
            return Describe(typeof(TService), implementationFactory, LifecycleKind.Transient, mode);
        }

        public ServiceDescriptor Transient(Type service, Func<IServiceProvider, object> implementationFactory, OverrideMode mode = OverrideMode.OverrideMany)
        {
            return Describe(service, implementationFactory, LifecycleKind.Transient, mode);
        }

        public ServiceDescriptor Scoped<TService, TImplementation>(OverrideMode mode = OverrideMode.OverrideMany)
        {
            return Describe<TService, TImplementation>(LifecycleKind.Scoped, mode);
        }

        public ServiceDescriptor Scoped(Type service, Type implementationType, OverrideMode mode = OverrideMode.OverrideMany)
        {
            return Describe(service, implementationType, LifecycleKind.Scoped, mode);
        }

        public ServiceDescriptor Scoped<TService>(Func<IServiceProvider, object> implementationFactory)
        {
            return Describe(typeof(TService), implementationFactory, LifecycleKind.Scoped);
        }

        public ServiceDescriptor Scoped(Type service, Func<IServiceProvider, object> implementationFactory)
        {
            return Describe(service, implementationFactory, LifecycleKind.Scoped);
        }

        public ServiceDescriptor Singleton<TService, TImplementation>(OverrideMode mode = OverrideMode.OverrideMany)
        {
            return Describe<TService, TImplementation>(LifecycleKind.Singleton, mode);
        }

        public ServiceDescriptor Singleton(Type service, Type implementationType, OverrideMode mode = OverrideMode.OverrideMany)
        {
            return Describe(service, implementationType, LifecycleKind.Singleton, mode);
        }

        public ServiceDescriptor Singleton<TService>(Func<IServiceProvider, object> implementationFactory, OverrideMode mode = OverrideMode.OverrideMany)
        {
            return Describe(typeof(TService), implementationFactory, LifecycleKind.Singleton, mode);
        }

        public ServiceDescriptor Singleton(Type serviceType, Func<IServiceProvider, object> implementationFactory, OverrideMode mode = OverrideMode.OverrideMany)
        {
            return Describe(serviceType, implementationFactory, LifecycleKind.Singleton, mode);
        }

        public ServiceDescriptor Instance<TService>(object implementationInstance, OverrideMode mode = OverrideMode.OverrideMany)
        {
            return Instance(typeof(TService), implementationInstance, mode);
        }

        public ServiceDescriptor Instance(Type serviceType, object implementationInstance, OverrideMode mode = OverrideMode.OverrideMany)
        {
            var implementationType = GetRemappedImplementationType(serviceType);
            if (implementationType != null)
            {
                return new ServiceDescriptor(serviceType, implementationType, LifecycleKind.Singleton, mode);
            }

            return new ServiceDescriptor(serviceType, implementationInstance, mode);
        }

        private ServiceDescriptor Describe<TService, TImplementation>(LifecycleKind lifecycle, OverrideMode mode)
        {
            return Describe(
                typeof(TService),
                typeof(TImplementation),
                lifecycle: lifecycle,
                mode: mode);
        }

        public ServiceDescriptor Describe(Type serviceType, Type implementationType, LifecycleKind lifecycle, OverrideMode mode = OverrideMode.DefaultMany)
        {
            implementationType = GetRemappedImplementationType(serviceType) ?? implementationType;

            return new ServiceDescriptor(serviceType, implementationType, lifecycle, mode);
        }

        public ServiceDescriptor Describe(Type serviceType, Func<IServiceProvider, object> implementationFactory, LifecycleKind lifecycle, OverrideMode mode = OverrideMode.DefaultMany)
        {
            var implementationType = GetRemappedImplementationType(serviceType);
            if (implementationType != null)
            {
                return new ServiceDescriptor(serviceType, implementationType, lifecycle, mode);
            }

            return new ServiceDescriptor(serviceType, implementationFactory, lifecycle, mode);
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
