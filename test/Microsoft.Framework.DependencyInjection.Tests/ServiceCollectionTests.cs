// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Framework.DependencyInjection.Tests.Fakes;
using Xunit;

namespace Microsoft.Framework.DependencyInjection
{
    public class ServiceCollectionTests
    {
        [Fact]
        public void Add_AddsDescriptorToServiceDescriptors()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var descriptor = new ServiceDescriptor(typeof(IFakeService), new FakeService());

            // Act
            serviceCollection.Add(descriptor);

            // Assert
            var result = Assert.Single(serviceCollection);
            Assert.Same(result, descriptor);
            result = Assert.Single(serviceCollection.ServiceDescriptors);
            Assert.Same(result, descriptor);
        }

        [Fact]
        public void Add_AddsMultipleDescriptorToServiceDescriptors()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var descriptor1 = new ServiceDescriptor(typeof(IFakeService), new FakeService());
            var descriptor2 = new ServiceDescriptor(typeof(IFactoryService), typeof(TransientFactoryService), LifecycleKind.Transient);
            var descriptors = new[] { descriptor1, descriptor2 };

            // Act
            serviceCollection.Add(descriptors);

            // Assert
            Assert.Equal(2, serviceCollection.Count());
            Assert.Equal(descriptors, serviceCollection);
            Assert.Equal(2, serviceCollection.ServiceDescriptors.Count);
            Assert.Equal<IServiceDescriptor>(descriptors, serviceCollection.ServiceDescriptors);
        }

        [Fact]
        public void ServiceDescriptors_AllowsRemovingPreviousRegisteredServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var descriptor1 = new ServiceDescriptor(typeof(IFakeService), new FakeService());
            var descriptor2 = new ServiceDescriptor(typeof(IFactoryService), typeof(TransientFactoryService), LifecycleKind.Transient);

            // Act
            serviceCollection.Add(descriptor1);
            serviceCollection.Add(descriptor2);
            serviceCollection.ServiceDescriptors.Remove(descriptor1);

            // Assert
            var result = Assert.Single(serviceCollection);
            Assert.Same(result, descriptor2);
            result = Assert.Single(serviceCollection.ServiceDescriptors);
            Assert.Same(result, descriptor2);
        }
    }
}