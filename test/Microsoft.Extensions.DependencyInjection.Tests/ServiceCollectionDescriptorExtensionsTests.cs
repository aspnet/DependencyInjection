// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class ServiceCollectionDescriptorExtensionsTest
    {
        [Fact]
        public void NonEnumerablServiceCanNotBeIEnumerableResolved()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddTransient(typeof(IFakeService), typeof(FakeService));
            var provider = collection.BuildServiceProvider();

            // Act
            var services = provider.GetService<IEnumerable<IFakeService>>();

            // Assert
            Assert.Null(services);
        }

        [Fact]
        public void Add_AddsDescriptorToServiceDescriptors()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var descriptor = ServiceDescriptor.Singleton(typeof(IFakeService), new FakeService());

            // Act
            serviceCollection.Add(descriptor);

            // Assert
            var result = Assert.Single(serviceCollection);
            Assert.Same(result, descriptor);
        }

        [Fact]
        public void Add_AddsMultipleDescriptorToServiceDescriptors()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var descriptor1 = ServiceDescriptor.Singleton(typeof(IFakeService), new FakeService());
            var descriptor2 = ServiceDescriptor.Describe(typeof(IFactoryService), typeof(TransientFactoryService), ServiceLifetime.Transient);

            // Act
            serviceCollection.Add(descriptor1);
            serviceCollection.Add(descriptor2);

            // Assert
            Assert.Equal(2, serviceCollection.Count);
            Assert.Equal(new[] { descriptor1, descriptor2 }, serviceCollection);
        }

        [Fact]
        public void ServiceDescriptors_AllowsRemovingPreviousRegisteredServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var descriptor1 = ServiceDescriptor.Singleton(typeof(IFakeService), new FakeService());
            var descriptor2 = ServiceDescriptor.Describe(typeof(IFactoryService), typeof(TransientFactoryService), ServiceLifetime.Transient);

            // Act
            serviceCollection.Add(descriptor1);
            serviceCollection.Add(descriptor2);
            serviceCollection.Remove(descriptor1);

            // Assert
            var result = Assert.Single(serviceCollection);
            Assert.Same(result, descriptor2);
        }
    }
}