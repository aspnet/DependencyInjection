﻿using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Builder;

namespace Microsoft.AspNet.DependencyInjection.Autofac
{
    public static class AutofacRegistration
    {
        public static void Populate(
                ContainerBuilder builder,
                IEnumerable<IServiceDescriptor> descriptors,
                params IEnumerable<IServiceDescriptor>[] moreDescriptors)
        {
            builder.RegisterType<AutofacServiceProvider>().As<IServiceProvider>();
            builder.RegisterType<AutofacServiceProvider>().As<IServiceScopeFactory>();

            Register(builder, descriptors);

            foreach (var serviceDescriptors in moreDescriptors)
            {
                Register(builder, serviceDescriptors);
            }
        }

        private static void Register(
                ContainerBuilder builder,
                IEnumerable<IServiceDescriptor> descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                if (descriptor.ImplementationType != null)
                {
                    builder
                        .RegisterType(descriptor.ImplementationType)
                        .As(descriptor.ServiceType)
                        .ConfigureLifecycle(descriptor.Lifecycle);
                }
                else
                {
                    builder
                        .RegisterInstance(descriptor.ImplementationInstance)
                        .As(descriptor.ServiceType)
                        .ConfigureLifecycle(descriptor.Lifecycle);
                }
            }
        }

        private static IRegistrationBuilder<object, T, SingleRegistrationStyle> ConfigureLifecycle<T>(
                this IRegistrationBuilder<object, T, SingleRegistrationStyle> registrationBuilder,
                LifecycleKind lifecycleKind)
        {
            switch (lifecycleKind)
            {
                case LifecycleKind.Singleton:
                    registrationBuilder.SingleInstance();
                    break;
                case LifecycleKind.Scoped:
                    registrationBuilder.InstancePerLifetimeScope();
                    break;
                case LifecycleKind.Transient:
                    registrationBuilder.InstancePerDependency();
                    break;
            }

            return registrationBuilder;
        }

        private class AutofacServiceProvider :
                IServiceProvider,
                IServiceScopeFactory
        {
            private readonly ILifetimeScope _lifetimeScope;

            public AutofacServiceProvider(ILifetimeScope lifetimeScope)
            {
                _lifetimeScope = lifetimeScope;
            }

            public object GetService(Type serviceType)
            {
                return _lifetimeScope.Resolve(serviceType);
            }

            public IServiceScope CreateScope()
            {
                return new AutofacServiceScope(_lifetimeScope.BeginLifetimeScope());
            }

            private void Dispose()
            {
                _lifetimeScope.Dispose();
            }

            private class AutofacServiceScope : IServiceScope
            {
                private readonly AutofacServiceProvider _serviceProvider;

                public AutofacServiceScope(ILifetimeScope lifetimeScope)
                {
                    _serviceProvider = new AutofacServiceProvider(lifetimeScope);
                }

                public IServiceProvider ServiceProvider
                {
                    get { return _serviceProvider; }
                }

                public void Dispose()
                {
                    _serviceProvider.Dispose();
                }
            }
        }
    }
}
