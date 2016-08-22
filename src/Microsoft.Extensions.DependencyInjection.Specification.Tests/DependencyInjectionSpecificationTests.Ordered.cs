// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Ordered;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Specification
{
    public abstract partial class DependencyInjectionSpecificationTests
    {
        [Fact]
        public void AddOrdered_AllowsResolvingEmptyIOrdered()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddOrdered<IFakeService>();
            var provider = CreateServiceProvider(collection);

            // Act
            var ordered = provider.GetService<IOrdered<IFakeService>>();

            // Assert
            Assert.Empty(ordered);
        }

        [Fact]
        public void AddOrdered_DoesNotResolveAsIEnumerable()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddOrdered<IFakeService>().AddTransient<FakeService>();
            var provider = CreateServiceProvider(collection);

            // Act
            var ordered = provider.GetService<IEnumerable<IFakeService>>();

            // Assert
            Assert.Null(ordered);
        }

        [Fact]
        public void AddOrdered_CachesInstances()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddOrdered<IFakeService>().AddTransient<FakeService>();
            var provider = CreateServiceProvider(collection);

            // Act
            var ordered = provider.GetService<IOrdered<IFakeService>>();
            var array1 = ordered.ToArray();
            var array2 = ordered.ToArray();

            // Assert
            Assert.Equal((IEnumerable<IFakeService>)array1, array2);
        }

        [Fact]
        public void AddOrdered_SupportsTwoInstancesOfSameType()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddOrdered<IFakeService>().AddTransient<FakeService>();
            collection.AddOrdered<IFakeService>().AddTransient<FakeService>();
            var provider = CreateServiceProvider(collection);

            // Act
            var ordered = provider.GetService<IOrdered<IFakeService>>();
            var array = ordered.ToArray();

            // Assert
            Assert.NotEqual(array[0], array[1]);
        }

        public static TheoryData AddOrderedOverloads
        {
            get
            {
                var serviceType = typeof(IFakeService);
                var implementationType = typeof(FakeServiceWithId);
                return new TheoryData<Action<IServiceCollection>>
                {
                    collection =>
                    {
                        collection.AddOrdered<IFakeService>().AddTransient<FakeServiceWithId>();
                        collection.AddOrdered<IFakeService>().AddTransient(_ => new FakeServiceWithId(1));
                        collection.AddOrdered<IFakeService>().AddSingleton(new FakeServiceWithId(2));
                    },
                    collection =>
                    {
                        collection.AddOrdered(serviceType).AddTransient(implementationType);
                        collection.AddOrdered(serviceType).AddTransient(_ => new FakeServiceWithId(1));
                        collection.AddOrdered(serviceType).AddSingleton(new FakeServiceWithId(2));
                    },
                    collection =>
                    {
                        collection.AddOrdered(serviceType).Add(ServiceDescriptor.Transient(serviceType, implementationType));
                        collection.AddOrdered(serviceType).Add(ServiceDescriptor.Transient(serviceType, _ => new FakeServiceWithId(1)));
                        collection.AddOrdered(serviceType).Add(ServiceDescriptor.Singleton(serviceType, new FakeServiceWithId(2)));
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(AddOrderedOverloads))]
        public void AddOrdered_SupportsAllServiceKinds(Action<IServiceCollection> addServices)
        {
            // Arrange
            var collection = new ServiceCollection();
            addServices(collection);
            var provider = CreateServiceProvider(collection);

            // Act
            var ordered = provider.GetService<IOrdered<IFakeService>>();
            var array = ordered.OfType<FakeServiceWithId>().ToArray();

            // Assert
            Assert.Equal(0, array[0].Id);
            Assert.Equal(1, array[1].Id);
            Assert.Equal(2, array[2].Id);
        }

        [Fact]
        public void RegistrationOrderIsPreservedWhenServicesAreIOrderedResolved()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddOrdered(typeof(IFakeMultipleService)).AddTransient(typeof(FakeOneMultipleService));
            collection.AddOrdered(typeof(IFakeMultipleService)).AddTransient(typeof(FakeTwoMultipleService));

            var provider = CreateServiceProvider(collection);

            collection = new ServiceCollection();
            collection.AddOrdered(typeof(IFakeMultipleService)).AddTransient(typeof(FakeTwoMultipleService));
            collection.AddOrdered(typeof(IFakeMultipleService)).AddTransient(typeof(FakeOneMultipleService));
            var providerReversed = CreateServiceProvider(collection);

            // Act
            var services = provider.GetService<IOrdered<IFakeMultipleService>>();
            var servicesReversed = providerReversed.GetService<IOrdered<IFakeMultipleService>>();

            // Assert
            Assert.Collection(services,
                service => Assert.IsType<FakeOneMultipleService>(service),
                service => Assert.IsType<FakeTwoMultipleService>(service));

            Assert.Collection(servicesReversed,
                service => Assert.IsType<FakeTwoMultipleService>(service),
                service => Assert.IsType<FakeOneMultipleService>(service));
        }

        [Fact]
        public void OrderedSupportsScoped()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddOrdered<IFakeScopedService>().AddScoped<FakeService>();
            var provider = CreateServiceProvider(collection);
            var cachedScopeFactory = provider.GetService<IServiceScopeFactory>();

            // Act
            for (var i = 0; i < 3; i++)
            {
                FakeService outerScopedService;
                using (var outerScope = cachedScopeFactory.CreateScope())
                {
                    var innerScopeFactory = outerScope.ServiceProvider.GetService<IServiceScopeFactory>();

                    FakeService innerScopedService;
                    using (var innerScope = innerScopeFactory.CreateScope())
                    {
                        outerScopedService = outerScope.ServiceProvider
                            .GetService<IOrdered<IFakeScopedService>>().First() as FakeService;

                        innerScopedService = innerScope.ServiceProvider
                            .GetService<IOrdered<IFakeScopedService>>().First() as FakeService;

                        // Assert
                        Assert.NotNull(outerScopedService);
                        Assert.NotNull(innerScopedService);
                        Assert.NotSame(outerScopedService, innerScopedService);
                    }

                    Assert.False(outerScopedService.Disposed);
                    Assert.True(innerScopedService.Disposed);
                }

                Assert.True(outerScopedService.Disposed);
            }
        }
    }
}