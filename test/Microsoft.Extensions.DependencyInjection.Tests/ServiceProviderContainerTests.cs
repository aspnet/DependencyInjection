// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection.Specification;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Microsoft.Extensions.DependencyInjection.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Ordered;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class ServiceProviderContainerTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection collection) =>
            collection.BuildServiceProvider();

        [Fact]
        public void RethrowOriginalExceptionFromConstructor()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<ClassWithThrowingEmptyCtor>();
            serviceCollection.AddTransient<ClassWithThrowingCtor>();
            serviceCollection.AddTransient<IFakeService, FakeService>();

            var provider = serviceCollection.BuildServiceProvider();

            var ex1 = Assert.Throws<Exception>(() => provider.GetService<ClassWithThrowingEmptyCtor>());
            Assert.Equal(nameof(ClassWithThrowingEmptyCtor), ex1.Message);

            var ex2 = Assert.Throws<Exception>(() => provider.GetService<ClassWithThrowingCtor>());
            Assert.Equal(nameof(ClassWithThrowingCtor), ex2.Message);
        }

        [Fact]
        public void DependencyWithPrivateConstructorIsIdentifiedAsPartOfException()
        {
            // Arrange
            var expectedMessage = $"A suitable constructor for type '{typeof(ClassWithPrivateCtor).FullName}' could not be located. "
                + "Ensure the type is concrete and services are registered for all parameters of a public constructor.";
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<ClassWithPrivateCtor>();
            serviceCollection.AddTransient<ClassDependsOnPrivateConstructorClass>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act and Assert
            var ex = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService<ClassDependsOnPrivateConstructorClass>());
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void AttemptingToResolveNonexistentServiceIndirectlyThrows()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddTransient<DependOnNonexistentService>();
            var provider = CreateServiceProvider(collection);

            // Act and Assert
            var ex = Assert.Throws<InvalidOperationException>(() => provider.GetService<DependOnNonexistentService>());
            Assert.Equal($"Unable to resolve service for type '{typeof(IFakeService)}' while attempting to activate " +
                $"'{typeof(DependOnNonexistentService)}'.", ex.Message);
        }

        [Fact]
        public void AttemptingToIEnumerableResolveNonexistentServiceIndirectlyThrows()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddEnumerable<DependOnNonexistentService>().AddTransient<DependOnNonexistentService>();
            var provider = CreateServiceProvider(collection);

            // Act and Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                provider.GetService<IEnumerable<DependOnNonexistentService>>());
            Assert.Equal($"Unable to resolve service for type '{typeof(IFakeService)}' while attempting to activate " +
                $"'{typeof(DependOnNonexistentService)}'.", ex.Message);
        }

        [Theory]
        // GenericTypeDefintion, Abstract GenericTypeDefintion
        [InlineData(typeof(IFakeOpenGenericService<>), typeof(AbstractFakeOpenGenericService<>))]
        // GenericTypeDefintion, Interface GenericTypeDefintion
        [InlineData(typeof(ICollection<>), typeof(IList<>))]
        // Implementation type is GenericTypeDefintion
        [InlineData(typeof(IList<int>), typeof(List<>))]
        // Implementation type is Abstract
        [InlineData(typeof(IFakeService), typeof(AbstractClass))]
        // Implementation type is Interface
        [InlineData(typeof(IFakeEveryService), typeof(IFakeService))]
        public void CreatingServiceProviderWithUnresolvableTypesThrows(Type serviceType, Type implementationType)
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(serviceType, implementationType);

            // Act and Assert
            var exception = Assert.Throws<ArgumentException>(() => serviceCollection.BuildServiceProvider());
            Assert.Equal(
                $"Cannot instantiate implementation type '{implementationType}' for service type '{serviceType}'.",
                exception.Message);
        }

        [Fact]
        public void NonEnumerableServiceCannotBeIEnumerableResolved()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddTransient(typeof(IFakeService), typeof(FakeService));
            var provider = CreateServiceProvider(collection);

            // Act
            var services = provider.GetService<IEnumerable<IFakeService>>();

            // Assert
            Assert.Null(services);
        }

        [Fact]
        public void IOrderedDoesNotResolveAsIEnumerable()
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

        private abstract class AbstractFakeOpenGenericService<T> : IFakeOpenGenericService<T>
        {
            public abstract T Value { get; }
        }
    }
}
