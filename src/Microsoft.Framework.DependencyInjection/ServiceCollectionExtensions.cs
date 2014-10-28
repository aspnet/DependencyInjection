// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Framework.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTransient([NotNull] this IServiceCollection collection, 
                                                      [NotNull] Type service, 
                                                      [NotNull] Type implementationType, 
                                                      OverrideMode mode = OverrideMode.OverrideMany)
        {
            return Add(collection, service, implementationType, LifecycleKind.Transient, mode);
        }

        public static IServiceCollection AddTransient([NotNull] this IServiceCollection collection,
                                                      [NotNull] Type service, 
                                                      [NotNull] Func<IServiceProvider, object> implementationFactory,
                                                      OverrideMode mode = OverrideMode.OverrideMany)
        {
            return Add(collection, service, implementationFactory, LifecycleKind.Transient, mode);
        }

        public static IServiceCollection AddScoped([NotNull] this IServiceCollection collection,
                                                   [NotNull] Type service,
                                                   [NotNull] Type implementationType,
                                                   OverrideMode mode = OverrideMode.OverrideMany)
        {
            return Add(collection, service, implementationType, LifecycleKind.Scoped, mode);
        }

        public static IServiceCollection AddScoped([NotNull] this IServiceCollection collection,
                                                   [NotNull] Type service,
                                                   [NotNull] Func<IServiceProvider, object> implementationFactory,
                                                   OverrideMode mode = OverrideMode.OverrideMany)
        {
            return Add(collection, service, implementationFactory, LifecycleKind.Scoped, mode);
        }

        public static IServiceCollection AddSingleton([NotNull] this IServiceCollection collection,
                                                      [NotNull] Type service,
                                                      [NotNull] Type implementationType,
                                                      OverrideMode mode = OverrideMode.OverrideMany)
        {
            return Add(collection, service, implementationType, LifecycleKind.Singleton, mode);
        }

        public static IServiceCollection AddSingleton([NotNull] this IServiceCollection collection,
                                                      [NotNull] Type service,
                                                      [NotNull] Func<IServiceProvider, object> implementationFactory,
                                                      OverrideMode mode = OverrideMode.OverrideMany)
        {
            return Add(collection, service, implementationFactory, LifecycleKind.Singleton, mode);
        }

        public static IServiceCollection AddInstance([NotNull] this IServiceCollection collection,
                                                     [NotNull] Type service,
                                                     [NotNull] object implementationInstance,
                                                     OverrideMode mode = OverrideMode.OverrideMany)
        {
            var serviceDescriptor = new ServiceDescriptor(service, implementationInstance, mode);
            return collection.Add(serviceDescriptor);
        }

        public static IServiceCollection AddTransient<TService, TImplementation>([NotNull] this IServiceCollection services,
                                                      OverrideMode mode = OverrideMode.OverrideMany)
        {
            return services.AddTransient(typeof(TService), typeof(TImplementation), mode);
        }

        public static IServiceCollection AddTransient([NotNull] this IServiceCollection services,
                                                      [NotNull] Type serviceType,
                                                      OverrideMode mode = OverrideMode.OverrideMany)
        {
            return services.AddTransient(serviceType, serviceType, mode);
        }

        public static IServiceCollection AddTransient<TService>([NotNull] this IServiceCollection services,
                                                      OverrideMode mode = OverrideMode.OverrideMany)
        {
            return services.AddTransient(typeof(TService), mode);
        }

        public static IServiceCollection AddTransient<TService>([NotNull] this IServiceCollection services,
                                                                [NotNull] Func<IServiceProvider, object> implementationFactory,
                                                                OverrideMode mode = OverrideMode.OverrideMany)
        {
            return services.AddTransient(typeof(TService), implementationFactory, mode);
        }

        public static IServiceCollection AddScoped<TService, TImplementation>([NotNull] this IServiceCollection services,
            OverrideMode mode = OverrideMode.OverrideMany)
        {
            return services.AddScoped(typeof(TService), typeof(TImplementation), mode);
        }

        public static IServiceCollection AddScoped([NotNull] this IServiceCollection services,
                                                   [NotNull] Type serviceType,
                                                   OverrideMode mode = OverrideMode.OverrideMany)
        {
            return services.AddScoped(serviceType, serviceType, mode);
        }

        public static IServiceCollection AddScoped<TService>([NotNull] this IServiceCollection services,
                                                             [NotNull] Func<IServiceProvider, object> implementationFactory,
                                                             OverrideMode mode = OverrideMode.OverrideMany)
        {
            return services.AddScoped(typeof(TService), implementationFactory, mode);
        }

        public static IServiceCollection AddScoped<TService>([NotNull] this IServiceCollection services,
            OverrideMode mode = OverrideMode.OverrideMany)
        {
            return services.AddScoped(typeof(TService), mode);
        }

        public static IServiceCollection AddSingleton<TService, TImplementation>([NotNull] this IServiceCollection services,
            OverrideMode mode = OverrideMode.OverrideMany)
        {
            return services.AddSingleton(typeof(TService), typeof(TImplementation), mode);
        }

        public static IServiceCollection AddSingleton([NotNull] this IServiceCollection services, 
                                                      [NotNull] Type serviceType,
                                                      OverrideMode mode = OverrideMode.OverrideMany)
        {
            return services.AddSingleton(serviceType, serviceType, mode);
        }

        public static IServiceCollection AddSingleton<TService>([NotNull] this IServiceCollection services,
            OverrideMode mode = OverrideMode.OverrideMany)
        {
            return services.AddSingleton(typeof(TService), mode);
        }

        public static IServiceCollection AddSingleton<TService>([NotNull] this IServiceCollection services,
                                                                [NotNull] Func<IServiceProvider, object> implementationFactory,
                                                                OverrideMode mode = OverrideMode.OverrideMany)

        {
            return services.AddSingleton(typeof(TService), implementationFactory, mode);
        }

        public static IServiceCollection AddInstance<TService>([NotNull] this IServiceCollection services, 
                                                               [NotNull] TService implementationInstance,
                                                               OverrideMode mode = OverrideMode.OverrideMany)
            where TService : class
        {
            return services.AddInstance(typeof(TService), implementationInstance, mode);
        }

        private static IServiceCollection Add(IServiceCollection collection,
                                              Type service,
                                              Type implementationType,
                                              LifecycleKind lifeCycle,
                                              OverrideMode mode)
        {
            var descriptor = new ServiceDescriptor(service, implementationType, lifeCycle, mode);
            return collection.Add(descriptor);
        }

        private static IServiceCollection Add(IServiceCollection collection,
                                              Type service,
                                              Func<IServiceProvider, object> implementationFactory,
                                              LifecycleKind lifeCycle,
                                              OverrideMode mode)
        {
            var descriptor = new ServiceDescriptor(service, implementationFactory, lifeCycle, mode);
            return collection.Add(descriptor);
        }
    }
}