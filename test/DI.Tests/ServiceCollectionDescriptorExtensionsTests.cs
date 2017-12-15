// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class ServiceCollectionDescriptorExtensionsTest
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
        }

        [Fact]
        public void Add_AddsMultipleDescriptorToServiceDescriptors()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var descriptor1 = new ServiceDescriptor(typeof(IFakeService), new FakeService());
            var descriptor2 = new ServiceDescriptor(typeof(IFactoryService), typeof(TransientFactoryService), ServiceLifetime.Transient);

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
            var descriptor1 = new ServiceDescriptor(typeof(IFakeService), new FakeService());
            var descriptor2 = new ServiceDescriptor(typeof(IFactoryService), typeof(TransientFactoryService), ServiceLifetime.Transient);

            // Act
            serviceCollection.Add(descriptor1);
            serviceCollection.Add(descriptor2);
            serviceCollection.Remove(descriptor1);

            // Assert
            var result = Assert.Single(serviceCollection);
            Assert.Same(result, descriptor2);
        }

        [Fact]
        public void Replace_ChangesImplementationType()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Act
            serviceCollection.AddSingleton<IFakeService, FakeService>();
            serviceCollection.Replace<IFakeService, FakeOneMultipleService>();

            // Assert
            var result = Assert.Single(serviceCollection);
            Assert.Equal(ServiceLifetime.Singleton, result.Lifetime);
            Assert.Equal(typeof(FakeOneMultipleService), result.ImplementationType);
        }

        [Fact]
        public void Replace_ThrowsIfNoServiceRegistered()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Act
            var exception = Assert.Throws<ArgumentException>(() =>
                serviceCollection.Replace<IFakeService, FakeOneMultipleService>());

            // Assert
            Assert.Equal("serviceType", exception.ParamName);
            Assert.Contains(typeof(IFakeService).FullName, exception.Message);
        }

        [Fact]
        public void TryGetDescriptors_GetsAllRegisteredDescriptors()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IFakeService, FakeService>();
            serviceCollection.AddScoped<IFakeScopedService, FakeService>();
            serviceCollection.AddSingleton<IFakeService, FakeOneMultipleService>();

            // Act
            var result = serviceCollection.TryGetDescriptors<IFakeService>(out var descriptors);

            // Assert
            Assert.True(result, "Did not find any matching descriptors.");
            Assert.Equal(2, descriptors.Count);
            Assert.All(descriptors, descriptor =>
            {
                Assert.Equal(typeof(IFakeService), descriptor.ServiceType);
                Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
            });
        }
    }
}