// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection.Specification;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Microsoft.Extensions.DependencyInjection.Tests.Fakes;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public abstract class ServiceProviderContainerTests : DependencyInjectionSpecificationTests
    {
        [Fact]
        public void RethrowOriginalExceptionFromConstructor()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<ClassWithThrowingEmptyCtor>();
            serviceCollection.AddTransient<ClassWithThrowingCtor>();
            serviceCollection.AddTransient<IFakeService, FakeService>();

            var provider = CreateServiceProvider(serviceCollection);

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
            var serviceProvider = CreateServiceProvider(serviceCollection);

            // Act and Assert
            var ex = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetServices<ClassDependsOnPrivateConstructorClass>());
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
            collection.AddTransient<DependOnNonexistentService>();
            var provider = CreateServiceProvider(collection);

            // Act and Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                provider.GetService<IEnumerable<DependOnNonexistentService>>());
            Assert.Equal($"Unable to resolve service for type '{typeof(IFakeService)}' while attempting to activate " +
                $"'{typeof(DependOnNonexistentService)}'.", ex.Message);
        }

        public static IEnumerable<object[]> ServiceProviderWithUnresolvableTypes()
        {
            // GenericTypeDefintion, Abstract GenericTypeDefintion
            yield return new object[]
            {
                typeof(IFakeOpenGenericService<>),
                typeof(AbstractFakeOpenGenericService<>),
                "Microsoft.Extensions.DependencyInjection.Specification.Fakes.IFakeOpenGenericService",
                "Microsoft.Extensions.DependencyInjection.Tests.ServiceProviderContainerTests+AbstractFakeOpenGenericService"
            };

            // GenericTypeDefintion, Interface GenericTypeDefintion
            yield return new object[]
            {
                typeof(ICollection<>),
                typeof(IList<>),
                "System.Collections.Generic.ICollection",
                "System.Collections.Generic.IList"
            };

            // Implementation type is GenericTypeDefintion
            yield return new object[]
            {
                typeof(IList<int>),
                typeof(List<>),
                "System.Collections.Generic.IList<int>",
                "System.Collections.Generic.List"
            };

            // Implementation type is Abstract
            yield return new object[]
            {
                typeof(IFakeService),
                typeof(AbstractClass),
                "Microsoft.Extensions.DependencyInjection.Specification.Fakes.IFakeService",
                "Microsoft.Extensions.DependencyInjection.Tests.Fakes.AbstractClass"
            };

            // Implementation type is Interface
            yield return new object[]
            {
                typeof(IFakeEveryService),
                typeof(IFakeService),
                "Microsoft.Extensions.DependencyInjection.Specification.Fakes.IFakeEveryService",
                "Microsoft.Extensions.DependencyInjection.Specification.Fakes.IFakeService"
            };
        }

        [Theory]
        [MemberData(nameof(ServiceProviderWithUnresolvableTypes))]
        public void CreatingServiceProviderWithUnresolvableTypesThrows(Type serviceType, Type implementationType, string serviceTypeName, string implementationTypeName)
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(serviceType, implementationType);

            // Act and Assert
            var exception = Assert.Throws<ArgumentException>(() => serviceCollection.BuildServiceProvider());
            Assert.Equal(
                $"Cannot instantiate implementation type '{implementationTypeName}' for service type '{serviceTypeName}'.",
                exception.Message);
        }

        [Fact]
        public void DoesNotDisposeSingletonInstances()
        {
            var disposable = new Disposable();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(disposable);

            var provider = CreateServiceProvider(serviceCollection);
            provider.GetService<Disposable>();

            ((IDisposable)provider).Dispose();

            Assert.False(disposable.Disposed);
        }

        [Fact]
        public void ResolvesServiceMixedServiceAndOptionalStructConstructorArguments()
        {
            var disposable = new Disposable();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IFakeService, FakeService>();
            serviceCollection.AddSingleton<ClassWithServiceAndOptionalArgsCtorWithStructs>();

            var provider = CreateServiceProvider(serviceCollection);
            var service = provider.GetService<ClassWithServiceAndOptionalArgsCtorWithStructs>();
        }

        private abstract class AbstractFakeOpenGenericService<T> : IFakeOpenGenericService<T>
        {
            public abstract T Value { get; }
        }

        private class Disposable : IDisposable
        {
            public bool Disposed { get; set; }

            public void Dispose()
            {
                Disposed = true;
            }
        }
    }
}
