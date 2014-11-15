// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.DependencyInjection.Tests.Fakes;
using Xunit;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Framework.DependencyInjection
{
    public class ServiceCollectionExtensionTest
    {
        private static readonly Func<IServiceProvider, object> _factory = _ => new object();
        private static readonly FakeService _instance = new FakeService();

        public static TheoryData AddImplementationTypeData
        {
            get
            {
                var serviceType = typeof(IFakeService);
                var implementationType = typeof(FakeService);
                return new TheoryData<Action<IServiceCollection>, Type, Type, LifecycleKind>
                {
                    { collection => collection.AddTransient(serviceType, implementationType), serviceType, implementationType, LifecycleKind.Transient },
                    { collection => collection.AddTransient<IFakeService, FakeService>(), serviceType, implementationType, LifecycleKind.Transient },
                    { collection => collection.AddTransient<IFakeService>(), serviceType, serviceType, LifecycleKind.Transient },
                    { collection => collection.AddTransient(implementationType), implementationType, implementationType, LifecycleKind.Transient },

                    { collection => collection.AddScoped(serviceType, implementationType), serviceType, implementationType, LifecycleKind.Scoped },
                    { collection => collection.AddScoped<IFakeService, FakeService>(), serviceType, implementationType, LifecycleKind.Scoped },
                    { collection => collection.AddScoped<IFakeService>(), serviceType, serviceType, LifecycleKind.Scoped },
                    { collection => collection.AddScoped(implementationType), implementationType, implementationType, LifecycleKind.Scoped },

                    { collection => collection.AddSingleton(serviceType, implementationType), serviceType, implementationType, LifecycleKind.Singleton },
                    { collection => collection.AddSingleton<IFakeService, FakeService>(), serviceType, implementationType, LifecycleKind.Singleton },
                    { collection => collection.AddSingleton<IFakeService>(), serviceType, serviceType, LifecycleKind.Singleton },
                    { collection => collection.AddSingleton(implementationType), implementationType, implementationType, LifecycleKind.Singleton },
                };
            }
        }

        [Theory]
        [MemberData(nameof(AddImplementationTypeData))]
        public void AddWithTypeAddsServiceWithRightLifecyle(Action<IServiceCollection> addTypeAction,
                                                            Type expectedServiceType,
                                                            Type expectedImplementationType,
                                                            LifecycleKind lifeCycle)
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            addTypeAction(collection);

            // Assert
            var descriptor = Assert.Single(collection);
            Assert.Equal(expectedServiceType, descriptor.ServiceType);
            Assert.Equal(expectedImplementationType, descriptor.ImplementationType);
            Assert.Equal(lifeCycle, descriptor.Lifecycle);
        }

        public static TheoryData AddImplementationFactoryData
        {
            get
            {
                var serviceType = typeof(IFakeService);

                return new TheoryData<Action<IServiceCollection>, LifecycleKind>
                {
                    { collection => collection.AddTransient<IFakeService>(_factory), LifecycleKind.Transient },
                    { collection => collection.AddTransient(serviceType, _factory), LifecycleKind.Transient },

                    { collection => collection.AddScoped<IFakeService>(_factory), LifecycleKind.Scoped },
                    { collection => collection.AddScoped(serviceType, _factory), LifecycleKind.Scoped },

                    { collection => collection.AddSingleton<IFakeService>(_factory), LifecycleKind.Singleton },
                    { collection => collection.AddSingleton(serviceType, _factory), LifecycleKind.Singleton },
                };
            }
        }

        [Theory]
        [MemberData(nameof(AddImplementationFactoryData))]
        public void AddWithFactoryAddsServiceWithRightLifecyle(Action<IServiceCollection> addAction,
                                                               LifecycleKind lifeCycle)
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            addAction(collection);

            // Assert
            var descriptor = Assert.Single(collection);
            Assert.Equal(typeof(IFakeService), descriptor.ServiceType);
            Assert.Same(_factory, descriptor.ImplementationFactory);
            Assert.Equal(lifeCycle, descriptor.Lifecycle);
        }

        public static TheoryData AddInstanceData
        {
            get
            {
                return new TheoryData<Action<IServiceCollection>>
                {
                    { collection => collection.AddInstance<IFakeService>(_instance) },
                    { collection => collection.AddInstance(typeof(IFakeService), _instance) },
                };
            }
        }

        [Theory]
        [MemberData(nameof(AddInstanceData))]
        public void AddInstance_AddsWithSingletonLifecycle(Action<IServiceCollection> addAction)
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            addAction(collection);

            // Assert
            var descriptor = Assert.Single(collection);
            Assert.Equal(typeof(IFakeService), descriptor.ServiceType);
            Assert.Same(_instance, descriptor.ImplementationInstance);
            Assert.Equal(LifecycleKind.Singleton, descriptor.Lifecycle);
        }

        [Fact]
        public void ImportAddsServices()
        {
            // Arrange
            var fallbackServices = new ServiceCollection();
            fallbackServices.AddSingleton<IFakeSingletonService, FakeService>();
            var instance = new FakeService();
            fallbackServices.AddInstance<IFakeServiceInstance>(instance);
            fallbackServices.AddTransient<IFakeService, FakeService>();

            var services = new ServiceCollection();
            services.Import(fallbackServices.BuildFallbackServiceProvider());

            // Act
            var provider = services.BuildServiceProvider();
            var singleton = provider.GetRequiredService<IFakeSingletonService>();
            var transient = provider.GetRequiredService<IFakeService>();

            // Assert
            Assert.Equal(singleton, provider.GetRequiredService<IFakeSingletonService>());
            Assert.NotEqual(transient, provider.GetRequiredService<IFakeService>());
            Assert.Equal(instance, provider.GetRequiredService<IFakeServiceInstance>());
        }

        [Fact]
        public void CanHideImportedServices()
        {
            // Arrange
            var fallbackServices = new ServiceCollection();
            var fallbackInstance = new FakeService();
            fallbackServices.AddInstance<IFakeService>(fallbackInstance);

            var services = new ServiceCollection();
            var realInstance = new FakeService();
            services.Import(fallbackServices.BuildFallbackServiceProvider());
            services.AddInstance<IFakeService>(realInstance);

            // Act
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.Equal(realInstance, provider.GetRequiredService<IFakeService>());
        }

        [Fact]
        public void ImportIgnoresAndDoesNotExplodeWithNoManifest()
        {
            // Arrange
            var fallbackServices = new ServiceCollection();
            fallbackServices.AddSingleton<IFakeSingletonService, FakeService>();
            var instance = new FakeService();
            fallbackServices.AddInstance<IFakeServiceInstance>(instance);
            fallbackServices.AddTransient<IFakeService, FakeService>();

            var services = new ServiceCollection();
            services.Import(fallbackServices.BuildServiceProvider());

            // Act
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.Null(provider.GetService<IFakeSingletonService>());
            Assert.Null(provider.GetService<IFakeService>());
            Assert.Null(provider.GetService<IFakeServiceInstance>());
        }

    }
}