// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.DependencyInjection.ServiceLookup;
using Microsoft.Framework.DependencyInjection.Tests.Fakes;
using Xunit;

namespace Microsoft.Framework.DependencyInjection
{
    public class ServiceManifestFacts
    {
        private class ServiceManifest : IServiceManifest
        {
            public ServiceManifest([NotNull] IEnumerable<Type> services)
            {
                Services = services;
            }

            public IEnumerable<Type> Services { get; private set; }
        }

        [Fact]
        public void ImportWithFallbackAddsServices()
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
        public void ImportWithCustomManifestAddsServices()
        {
            // Arrange
            var fallbackServices = new ServiceCollection();
            fallbackServices.AddSingleton<IFakeSingletonService, FakeService>();
            var instance = new FakeService();
            fallbackServices.AddInstance<IFakeServiceInstance>(instance);
            fallbackServices.AddTransient<IFakeService, FakeService>();

            fallbackServices.AddInstance<IServiceManifest>(new ServiceManifest(
                new Type[] { typeof(IFakeServiceInstance), typeof(IFakeService), typeof(IFakeSingletonService) }));

            var services = new ServiceCollection();
            services.Import(fallbackServices.BuildServiceProvider());

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
            fallbackServices.AddInstance<IServiceManifest>(new ServiceManifest(
                new Type[] { typeof(IFakeService) }));
            services.AddInstance<IFakeService>(realInstance);

            // Act
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.Equal(realInstance, provider.GetRequiredService<IFakeService>());
        }

        [Fact]
        public void ImportExplodesWithNoManifest()
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

        [Theory]
        [InlineData(typeof(IList<string>), typeof(List<string>), true)]
        [InlineData(typeof(IDictionary<string, int>), typeof(Dictionary<string, int>), true)]
        [InlineData(typeof(IServiceManifest), typeof(ServiceManifest), false)]
        [InlineData(typeof(IServiceProvider), typeof(ServiceProvider), false)]
        [InlineData(typeof(IList), typeof(List<string>), true)]
        [InlineData(typeof(object), typeof(int), true)]
        [InlineData(typeof(ITypeActivator), typeof(TypeActivator), true)]
        [InlineData(typeof(IEatGenerics<>), typeof(GenericsYum<>), false)]
        public void ManifestGeneration(Type service, Type impl, bool allowed)
        {
            // Arrange
            var fallbackServices = new ServiceCollection();
            fallbackServices.AddTransient(service, impl);

            // Act
            var manifest = fallbackServices.BuildFallbackServiceProvider().GetRequiredService<IServiceManifest>();

            // Assert
            Assert.NotNull(manifest);
            Assert.Equal(allowed, manifest.Services.Any(t => t == service));
        }

        private interface IEatGenerics<T> { }

        private class GenericsYum<T> : IEatGenerics<T> { }
    }
}