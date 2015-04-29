// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using SimpleInjector;
using SimpleInjector.Extensions;
using SimpleInjector.Extensions.LifetimeScoping;
using System;
using System.Collections.Generic;

namespace Microsoft.Framework.DependencyInjection.SimpleInjector
{
    public static class SimpleInjectorRegistration
    {
        public static void Populate(this Container container, IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            container.Register<IServiceProvider>(() => container);
            container.Register<IServiceScopeFactory>(() => new SimpleInjectorServiceScopeFactory(container));

            Register(container, serviceDescriptors);
        }

        private static void Register(Container container, IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            foreach (var descriptor in serviceDescriptors)
            {
                if (descriptor.ImplementationType != null)
                {
                    // Test if an open generic type is being registered.
                    var serviceTypeInfo = descriptor.ServiceType.GetType();
                    if (serviceTypeInfo.IsGenericTypeDefinition)
                    {
                        container.RegisterOpenGeneric(descriptor.ServiceType,
                            descriptor.ImplementationType,
                            descriptor.Lifetime.ToLifestyle());
                    }
                    else
                    {
                        container.Register(descriptor.ServiceType,
                            descriptor.ImplementationType,
                            descriptor.Lifetime.ToLifestyle());
                    }
                }
                else if (descriptor.ImplementationFactory != null)
                {
                    container.Register(descriptor.ServiceType, () => descriptor.ImplementationFactory(container), descriptor.Lifetime.ToLifestyle());
                }
                else
                {
                    container.Register(descriptor.ServiceType, () => descriptor.ImplementationInstance, descriptor.Lifetime.ToLifestyle());
                }
            }
        }
        
        private static Lifestyle ToLifestyle(this ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    return Lifestyle.Singleton;

                case ServiceLifetime.Transient:
                    return Lifestyle.Transient;

                case ServiceLifetime.Scoped:
                    return new LifetimeScopeLifestyle();
            }

            return Lifestyle.Transient;
        }

        private class SimpleInjectorServiceScopeFactory : IServiceScopeFactory
        {
            private readonly Container _container;

            public SimpleInjectorServiceScopeFactory(Container container)
            {
                _container = container;
            }

            public IServiceScope CreateScope()
            {
                return new SimpleInjectorServiceScope(_container);
            }
        }

        private class SimpleInjectorServiceScope : IServiceScope
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly LifetimeScope _scope;

            public SimpleInjectorServiceScope(Container container)
            {
                _serviceProvider = container;
                _scope = container.BeginLifetimeScope();

            }

            public IServiceProvider ServiceProvider
            {
                get
                {
                    return _serviceProvider;
                }
            }

            public void Dispose()
            {
                _scope.Dispose();
            }
        }
    }
}