// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Framework.DependencyInjection.Tests.Fakes;
using Xunit;

namespace Microsoft.Framework.DependencyInjection.Tests
{
    public class ServiceProviderContainerTests : ScopingContainerTestBase
    {
        protected override IServiceProvider CreateContainer()
        {
            return TestServices.DefaultServices().BuildServiceProvider();
        }

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

        [Theory]
        // GenericTypeDefintion, Abstract GenericTypeDefintion
        [InlineData(typeof(ICollection<>), typeof(System.Collections.ObjectModel.Collection<>))]
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
            var provider = serviceCollection.BuildServiceProvider();

            // Act and Assert
            var exception = Assert.Throws<ArgumentException>(() => provider.GetService<IList<int>>());
            Assert.Equal(
                $"Cannot instantiate implementation type '{implementationType}' for service type '{serviceType}'.",
                exception.Message);
        }
    }
}
