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
                                                      bool isFallback = false)
        {
            return Add(collection, service, implementationType, LifecycleKind.Transient, isFallback);
        }

        public static IServiceCollection AddTransient([NotNull] this IServiceCollection collection,
                                                      [NotNull] Type service, 
                                                      [NotNull] Func<IServiceProvider, object> implementationFactory,
                                                      bool isFallback = false)
        {
            return Add(collection, service, implementationFactory, LifecycleKind.Transient, isFallback);
        }

        public static IServiceCollection AddScoped([NotNull] this IServiceCollection collection,
                                                   [NotNull] Type service,
                                                   [NotNull] Type implementationType,
                                                   bool isFallback = false)
        {
            return Add(collection, service, implementationType, LifecycleKind.Scoped, isFallback);
        }

        public static IServiceCollection AddScoped([NotNull] this IServiceCollection collection,
                                                   [NotNull] Type service,
                                                   [NotNull] Func<IServiceProvider, object> implementationFactory,
                                                   bool isFallback = false)
        {
            return Add(collection, service, implementationFactory, LifecycleKind.Scoped, isFallback);
        }

        public static IServiceCollection AddSingleton([NotNull] this IServiceCollection collection,
                                                      [NotNull] Type service,
                                                      [NotNull] Type implementationType,
                                                      bool isFallback = false)
        {
            return Add(collection, service, implementationType, LifecycleKind.Singleton, isFallback);
        }

        public static IServiceCollection AddSingleton([NotNull] this IServiceCollection collection,
                                                      [NotNull] Type service,
                                                      [NotNull] Func<IServiceProvider, object> implementationFactory,
                                                      bool isFallback = false)
        {
            return Add(collection, service, implementationFactory, LifecycleKind.Singleton, isFallback);
        }

        public static IServiceCollection AddInstance([NotNull] this IServiceCollection collection,
                                                     [NotNull] Type service,
                                                     [NotNull] object implementationInstance,
                                                     bool isFallback = false)
        {
            var serviceDescriptor = new ServiceDescriptor(service, implementationInstance, isFallback);
            return collection.Add(serviceDescriptor);
        }

        public static IServiceCollection AddTransient<TService, TImplementation>([NotNull] this IServiceCollection services,
                                                      bool isFallback = false)
        {
            return services.AddTransient(typeof(TService), typeof(TImplementation), isFallback);
        }

        public static IServiceCollection AddTransient([NotNull] this IServiceCollection services,
                                                      [NotNull] Type serviceType,
                                                      bool isFallback = false)
        {
            return services.AddTransient(serviceType, serviceType, isFallback);
        }

        public static IServiceCollection AddTransient<TService>([NotNull] this IServiceCollection services,
                                                      bool isFallback = false)
        {
            return services.AddTransient(typeof(TService), isFallback);
        }

        public static IServiceCollection AddTransient<TService>([NotNull] this IServiceCollection services,
                                                                [NotNull] Func<IServiceProvider, object> implementationFactory,
                                                                bool isFallback = false)
        {
            return services.AddTransient(typeof(TService), implementationFactory, isFallback);
        }

        public static IServiceCollection AddScoped<TService, TImplementation>([NotNull] this IServiceCollection services,
            bool isFallback = false)
        {
            return services.AddScoped(typeof(TService), typeof(TImplementation), isFallback);
        }

        public static IServiceCollection AddScoped([NotNull] this IServiceCollection services,
                                                   [NotNull] Type serviceType,
                                                   bool isFallback = false)
        {
            return services.AddScoped(serviceType, serviceType, isFallback);
        }

        public static IServiceCollection AddScoped<TService>([NotNull] this IServiceCollection services,
                                                             [NotNull] Func<IServiceProvider, object> implementationFactory,
                                                             bool isFallback = false)
        {
            return services.AddScoped(typeof(TService), implementationFactory, isFallback);
        }

        public static IServiceCollection AddScoped<TService>([NotNull] this IServiceCollection services,
            bool isFallback = false)
        {
            return services.AddScoped(typeof(TService), isFallback);
        }

        public static IServiceCollection AddSingleton<TService, TImplementation>([NotNull] this IServiceCollection services,
            bool isFallback = false)
        {
            return services.AddSingleton(typeof(TService), typeof(TImplementation), isFallback);
        }

        public static IServiceCollection AddSingleton([NotNull] this IServiceCollection services, 
                                                      [NotNull] Type serviceType,
                                                      bool isFallback = false)
        {
            return services.AddSingleton(serviceType, serviceType, isFallback);
        }

        public static IServiceCollection AddSingleton<TService>([NotNull] this IServiceCollection services,
            bool isFallback = false)
        {
            return services.AddSingleton(typeof(TService), isFallback);
        }

        public static IServiceCollection AddSingleton<TService>([NotNull] this IServiceCollection services,
                                                                [NotNull] Func<IServiceProvider, object> implementationFactory,
                                                                bool isFallback = false)

        {
            return services.AddSingleton(typeof(TService), implementationFactory, isFallback);
        }

        public static IServiceCollection AddInstance<TService>([NotNull] this IServiceCollection services, 
                                                               [NotNull] TService implementationInstance,
                                                               bool isFallback = false)
            where TService : class
        {
            return services.AddInstance(typeof(TService), implementationInstance, isFallback);
        }

        private static IServiceCollection Add(IServiceCollection collection,
                                              Type service,
                                              Type implementationType,
                                              LifecycleKind lifeCycle,
                                              bool isFallback)
        {
            var descriptor = new ServiceDescriptor(service, implementationType, lifeCycle, isFallback);
            return collection.Add(descriptor);
        }

        private static IServiceCollection Add(IServiceCollection collection,
                                              Type service,
                                              Func<IServiceProvider, object> implementationFactory,
                                              LifecycleKind lifeCycle,
                                              bool isFallback)
        {
            var descriptor = new ServiceDescriptor(service, implementationFactory, lifeCycle, isFallback);
            return collection.Add(descriptor);
        }
    }
}