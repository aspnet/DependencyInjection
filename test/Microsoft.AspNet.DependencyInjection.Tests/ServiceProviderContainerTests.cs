﻿using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.AspNet.DependencyInjection.Tests.Fakes;
using Xunit;

namespace Microsoft.AspNet.DependencyInjection.Tests
{
    public class ServiceProviderContainerTests : AllContainerTestsBase
    {
        protected override IServiceProvider CreateContainer()
        {
            return new ServiceCollection()
                .Add(TestServices.DefaultServices())
                .BuildServiceProvider(new FakeFallbackServiceProvider());
        }

        [Fact]
        public void SingletonServiceCanBeResolved()
        {
            var container = CreateContainer();

            var service1 = container.GetService<IFakeSingletonService>();
            var service2 = container.GetService<IFakeSingletonService>();

            Assert.NotNull(service1);
            Assert.Equal(service1, service2);
        }

        [Fact]
        public void ScopedServiceCanBeResolved()
        {
            var container = CreateContainer();

            var scopeFactory = container.GetService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var containerScopedService = container.GetService<IFakeScopedService>();
                var scopedService1 = scope.ServiceProvider.GetService<IFakeScopedService>();
                var scopedService2 = scope.ServiceProvider.GetService<IFakeScopedService>();

                Assert.NotEqual(containerScopedService, scopedService1);
                Assert.Equal(scopedService1, scopedService2);
            }
        }

        [Fact]
        public void NestedScopedServiceCanBeResolved()
        {
            var container = CreateContainer();

            var outerScopeFactory = container.GetService<IServiceScopeFactory>();
            using (var outerScope = outerScopeFactory.CreateScope())
            {
                var innerScopeFactory = outerScope.ServiceProvider.GetService<IServiceScopeFactory>();
                using (var innerScope = innerScopeFactory.CreateScope())
                {
                    var outerScopedService = outerScope.ServiceProvider.GetService<IFakeScopedService>();
                    var innerScopedService = innerScope.ServiceProvider.GetService<IFakeScopedService>();

                    Assert.NotEqual(outerScopedService, innerScopedService);
                }
            }
        }

        [Fact]
        public void DisposingScopeDisposesService()
        {
            var container = CreateContainer();
            FakeService disposableService;

            var scopeFactory = container.GetService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                disposableService = (FakeService)scope.ServiceProvider.GetService<IFakeScopedService>();

                Assert.False(disposableService.Disposed);
            }

            Assert.True(disposableService.Disposed);
        }

        [Fact]
        public void ServicesCanBeResolvedFromFallbackServiceProvider()
        {
            var container = CreateContainer();

            var service = container.GetService<string>();

            Assert.Equal("FakeFallbackServiceProvider", service);
        }

        [Fact]
        public void ServicesFromFallbackServicProviderCanBeReplaced()
        {
            var container = CreateContainer();

            var service = container.GetService<IFakeFallbackService>();

            Assert.Equal("FakeServiceSimpleMethod", service.SimpleMethod());
        }

        [Fact]
        public void ServicesCanBeAddedToServicesFromFallbackServiceProvider()
        {
            var container = CreateContainer();

            var services = container.GetService<IEnumerable<IFakeFallbackService>>();
            var messages = services.Select(service => service.SimpleMethod());

            Assert.Equal(2, services.Count());
            Assert.Contains("FakeServiceSimpleMethod", messages);
            Assert.Contains("FakeFallbackServiceProvider", messages);
        }
    }
}
