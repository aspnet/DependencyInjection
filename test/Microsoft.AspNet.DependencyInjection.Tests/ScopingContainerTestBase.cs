﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.DependencyInjection.Tests.Fakes;
using Xunit;

namespace Microsoft.AspNet.DependencyInjection.Tests
{
    public abstract class ScopingContainerTestBase : AllContainerTestsBase
    {
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
        public void SingletonServiceCanBeResolvedFromScope()
        {
            var container = CreateContainer();

            var scopeFactory = container.GetService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var service1 = container.GetService<IFakeSingletonService>();
                var service2 = scope.ServiceProvider.GetService<IFakeSingletonService>();

                Assert.NotNull(service1);
                Assert.Equal(service1, service2);
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
        public void ServicesFromFallbackServiceProviderCanBeReplaced()
        {
            var container = CreateContainer();

            var service = container.GetService<IFakeFallbackService>();

            Assert.Equal("FakeServiceSimpleMethod", service.SimpleMethod());
        }

        [Fact]
        public void ServicesFromFallbackServiceProviderCanBeReplacedAndIEnumerableResolved()
        {
            var container = CreateContainer();

            var services = container.GetService<IEnumerable<IFakeFallbackService>>();
            var messages = services.Select(service => service.SimpleMethod());

            Assert.Equal(1, services.Count());
            Assert.Contains("FakeServiceSimpleMethod", messages);
        }
    }
}
