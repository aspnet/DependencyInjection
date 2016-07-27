// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Xunit;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public class ServiceCollectionOrderedServiceExtensionsTest
    {
        [Fact]
        public void AddOrdered_AllowsResolvingEmptyIOrdered()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddOrdered<IFakeService>();
            var provider = collection.BuildServiceProvider();

            // Act
            var ordered = provider.GetService<IOrdered<IFakeService>>();

            // Assert
            Assert.Empty(ordered);
        }

        [Fact]
        public void AddOrdered_AllowsResolvingAsIEnumerable()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddOrdered<IFakeService, FakeService>();
            var provider = collection.BuildServiceProvider();

            // Act
            var ordered = provider.GetService<IEnumerable<IFakeService>>();

            // Assert
            Assert.IsType<FakeService>(ordered.Single());
        }

        [Fact]
        public void AddOrdered_CachesInstances()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddOrdered<IFakeService, FakeService>();
            var provider = collection.BuildServiceProvider();

            // Act
            var ordered = provider.GetService<IEnumerable<IFakeService>>();
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
            collection.AddOrdered<IFakeService, FakeService>();
            collection.AddOrdered<IFakeService, FakeService>();
            var provider = collection.BuildServiceProvider();

            // Act
            var ordered = provider.GetService<IEnumerable<IFakeService>>();
            var array = ordered.ToArray();

            // Assert
            Assert.NotEqual(array[0], array[1]);
        }
    }
}