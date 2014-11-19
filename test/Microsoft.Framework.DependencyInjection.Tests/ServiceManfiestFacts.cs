// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.DependencyInjection.ServiceLookup;
using Microsoft.Framework.DependencyInjection.Tests.Fakes;
using Xunit;

namespace Microsoft.Framework.DependencyInjection
{
    public class ServiceManifestFacts
    {
        [Fact]
        public void ImportAddsServices()
        {
            // Arrange
            var fallbackServices = new ServiceCollection();
            fallbackServices.AddSingleton<IFakeSingletonService, FakeService>();
            var instance = new FakeService();
            fallbackServices.AddInstance<IFakeServiceInstance>(instance);
            fallbackServices.AddTransient<IFakeService, FakeService>();
            fallbackServices.AddSingleton<IFactoryService>(serviceProvider => instance);

            fallbackServices.AddInstance<IServiceManifest>(new ServiceManifest(
                new Type[] { typeof(IFakeServiceInstance), typeof(IFakeService), typeof(IFakeSingletonService) }));

            var services = new ServiceCollection();
            services.Import(fallbackServices.BuildServiceProvider());

            // Act
            var provider = services.BuildServiceProvider();
            var singleton = provider.GetRequiredService<IFakeSingletonService>();
            var transient = provider.GetRequiredService<IFakeService>();
            var factory = provider.GetRequiredService<IFactoryService>();

            // Assert
            Assert.Same(singleton, provider.GetRequiredService<IFakeSingletonService>());
            Assert.NotSame(transient, provider.GetRequiredService<IFakeService>());
            Assert.Same(instance, provider.GetRequiredService<IFakeServiceInstance>());
            Assert.Same(instance, provider.GetRequiredService<IFactoryService>());
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
            fallbackServices.AddInstance<IServiceManifest>(new ServiceManifest(
                new Type[] { typeof(IFakeService) }));
            services.AddInstance<IFakeService>(realInstance);

            // Act
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.Equal(realInstance, provider.GetRequiredService<IFakeService>());
        }

        [Fact]
        public void ImportThrowsWithNoManifest()
        {
            // Arrange
            var fallbackServices = new ServiceCollection();
            fallbackServices.AddSingleton<IFakeSingletonService, FakeService>();
            var instance = new FakeService();
            fallbackServices.AddInstance<IFakeServiceInstance>(instance);
            fallbackServices.AddTransient<IFakeService, FakeService>();

            var services = new ServiceCollection();
            // Act
            var exp = Assert.Throws<Exception>(() => services.Import(fallbackServices.BuildServiceProvider()));


            // Assert
            Assert.True(exp.Message.Contains("No service for type 'Microsoft.Framework.DependencyInjection.ServiceLookup.IServiceManifest'"));
        }

        private class ServiceManifest : IServiceManifest
        {
            public ServiceManifest([NotNull] IEnumerable<Type> services)
            {
                Services = services;
            }

            public IEnumerable<Type> Services { get; private set; }
        }
    }
}