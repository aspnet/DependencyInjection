// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;

namespace Microsoft.Framework.DependencyInjection.Windsor
{
    public static class WindsorRegistration
    {
        public static void Populate(
                this IWindsorContainer container,
                IEnumerable<IServiceDescriptor> descriptors)
        {
            container.Register(Component.For<IWindsorContainer>().Instance(container));
            container.Register(Component.For<IServiceProvider>().ImplementedBy<WindsorServiceProvider>().LifestyleTransient());
            container.Register(Component.For<IServiceScopeFactory>().ImplementedBy<WindsorServiceScopeFactory>());

            // Necessary to resolve types within collections
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));

            Register(container, descriptors);
        }

        private static void Register(
                IWindsorContainer container,
                IEnumerable<IServiceDescriptor> descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                if (descriptor.ImplementationType != null)
                {
                    container.Register(Component.For(descriptor.ServiceType)
                        .NamedAutomatically(descriptor.ServiceType.FullName + Guid.NewGuid().ToString())
                        .ImplementedBy(descriptor.ImplementationType)
                        .ConfigureLifecycle(descriptor.Lifecycle)
                        .IsDefault());
                }
                else if (descriptor.ImplementationFactory != null)
                {
                    var savedDescriptor = descriptor;
                    container.Register(Component.For(descriptor.ServiceType)
                        .NamedAutomatically(descriptor.ServiceType.FullName + Guid.NewGuid().ToString())
                        .UsingFactoryMethod(c =>
                        {
                            var builderProvider = container.Resolve<IServiceProvider>();
                            return savedDescriptor.ImplementationFactory(builderProvider);
                        })
                        .ConfigureLifecycle(descriptor.Lifecycle)
                        .IsDefault());
                }
                else
                {
                    container.Register(Component.For(descriptor.ServiceType)
                        .NamedAutomatically(descriptor.ServiceType.FullName + Guid.NewGuid().ToString())
                        .Instance(descriptor.ImplementationInstance)
                        .ConfigureLifecycle(descriptor.Lifecycle)
                        .IsDefault());
                }
            }
        }

        private static ComponentRegistration<object> ConfigureLifecycle(
                this ComponentRegistration<object> registrationBuilder,
                LifecycleKind lifecycleKind)
        {
            switch (lifecycleKind)
            {
                case LifecycleKind.Singleton:
                    registrationBuilder.LifestyleSingleton();
                    break;
                case LifecycleKind.Scoped:
                    registrationBuilder.LifestyleScoped();
                    break;
                case LifecycleKind.Transient:
                    registrationBuilder.LifestyleTransient();
                    break;
            }

            return registrationBuilder;
        }

        private class WindsorServiceProvider : IServiceProvider
        {
            private readonly IWindsorContainer _container;

            public WindsorServiceProvider(IWindsorContainer container)
            {
                _container = container;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    var typeToResolve = serviceType.GetGenericArguments()[0];
                    if (_container.Kernel.HasComponent(typeToResolve))
                    {
                        return _container.ResolveAll(typeToResolve);
                    }

                    var listType = typeof(List<>).MakeGenericType(typeToResolve);
                    return Activator.CreateInstance(listType);
                }

                if (_container.Kernel.HasComponent(serviceType))
                {
                    return _container.Resolve(serviceType);
                }

                return null;
            }
        }

        private class WindsorServiceScopeFactory : IServiceScopeFactory
        {
            private readonly IWindsorContainer _container;

            public WindsorServiceScopeFactory(IWindsorContainer container)
            {
                _container = container;
            }

            public IServiceScope CreateScope()
            {
                return new WindsorServiceScope(_container);
            }
        }

        private class WindsorServiceScope : IServiceScope
        {
            private readonly IDisposable _scope;
            private IServiceProvider _serviceProvider;

            public WindsorServiceScope(IWindsorContainer container)
            {
                _scope = container.BeginScope();
                _serviceProvider = container.Resolve<IServiceProvider>();
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