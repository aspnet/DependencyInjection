// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Tests.Fakes;
using Xunit;
using static Microsoft.Extensions.Internal.TypeNameHelper;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CircularDependencyTests
    {
        [Fact]
        public void SelfCircularDependency()
        {
            var expectedMessage = "A circular dependency was detected for the service of type " +
                                  $"'{GetTypeDisplayName(typeof(SelfCircularDependency))}'." +
                                  Environment.NewLine +
                                  $"{GetTypeDisplayName(typeof(SelfCircularDependency))} -> " +
                                  $"{GetTypeDisplayName(typeof(SelfCircularDependency))}";

            var serviceProvider = new ServiceCollection()
                .AddTransient<SelfCircularDependency>()
                .BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetRequiredService<SelfCircularDependency>());

            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void SelfCircularDependencyInEnumerable()
        {
            var expectedMessage = "A circular dependency was detected for the service of type " +
                                  $"'{GetTypeDisplayName(typeof(SelfCircularDependency))}'." +
                                  Environment.NewLine +
                                  $"{GetTypeDisplayName(typeof(IEnumerable<SelfCircularDependency>))} -> " +
                                  $"{GetTypeDisplayName(typeof(SelfCircularDependency))} -> " +
                                  $"{GetTypeDisplayName(typeof(SelfCircularDependency))}";

            var serviceProvider = new ServiceCollection()
                .AddTransient<SelfCircularDependency>()
                .BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetRequiredService<IEnumerable<SelfCircularDependency>>());

            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void SelfCircularDependencyGenericDirect()
        {
            var expectedMessage = "A circular dependency was detected for the service of type " +
                                  $"'{GetTypeDisplayName(typeof(SelfCircularDependencyGeneric<string>))}'." +
                                  Environment.NewLine +
                                  $"{GetTypeDisplayName(typeof(SelfCircularDependencyGeneric<string>))} -> " +
                                  $"{GetTypeDisplayName(typeof(SelfCircularDependencyGeneric<string>))}";

            var serviceProvider = new ServiceCollection()
                .AddTransient<SelfCircularDependencyGeneric<string>>()
                .BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetRequiredService<SelfCircularDependencyGeneric<string>>());

            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void SelfCircularDependencyGenericIndirect()
        {
            var expectedMessage = "A circular dependency was detected for the service of type " +
                                  $"'{GetTypeDisplayName(typeof(SelfCircularDependencyGeneric<string>))}'." +
                                  Environment.NewLine +
                                  $"{GetTypeDisplayName(typeof(SelfCircularDependencyGeneric<int>))} -> " +
                                  $"{GetTypeDisplayName(typeof(SelfCircularDependencyGeneric<string>))} -> " +
                                  $"{GetTypeDisplayName(typeof(SelfCircularDependencyGeneric<string>))}";

            var serviceProvider = new ServiceCollection()
                .AddTransient<SelfCircularDependencyGeneric<int>>()
                .AddTransient<SelfCircularDependencyGeneric<string>>()
                .BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetRequiredService<SelfCircularDependencyGeneric<int>>());

            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void NoCircularDependencyGeneric()
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton(new SelfCircularDependencyGeneric<string>())
                .AddTransient<SelfCircularDependencyGeneric<int>>()
                .BuildServiceProvider();

            // This will not throw because we are creating an instance of the first time
            // using the parameterless constructor which has no circular dependency
            var resolvedService = serviceProvider.GetRequiredService<SelfCircularDependencyGeneric<int>>();
            Assert.NotNull(resolvedService);
        }

        [Fact]
        public void SelfCircularDependencyWithInterface()
        {
            var expectedMessage = "A circular dependency was detected for the service of type " +
                                  $"'{GetTypeDisplayName(typeof(ISelfCircularDependencyWithInterface))}'." +
                                  Environment.NewLine +
                                  $"{GetTypeDisplayName(typeof(SelfCircularDependencyWithInterface))} -> " +
                                  $"{GetTypeDisplayName(typeof(ISelfCircularDependencyWithInterface))}" +
                                  $"({GetTypeDisplayName(typeof(SelfCircularDependencyWithInterface))}) -> " +
                                  $"{GetTypeDisplayName(typeof(ISelfCircularDependencyWithInterface))}";

            var serviceProvider = new ServiceCollection()
                .AddTransient<ISelfCircularDependencyWithInterface, SelfCircularDependencyWithInterface>()
                .AddTransient<SelfCircularDependencyWithInterface>()
                .BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetRequiredService<SelfCircularDependencyWithInterface>());

            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void DirectCircularDependency()
        {
            var expectedMessage = "A circular dependency was detected for the service of type " +
                                  $"'{GetTypeDisplayName(typeof(DirectCircularDependencyA))}'." +
                                  Environment.NewLine +
                                  $"{GetTypeDisplayName(typeof(DirectCircularDependencyA))} -> " +
                                  $"{GetTypeDisplayName(typeof(DirectCircularDependencyB))} -> " +
                                  $"{GetTypeDisplayName(typeof(DirectCircularDependencyA))}";

            var serviceProvider = new ServiceCollection()
                .AddSingleton<DirectCircularDependencyA>()
                .AddSingleton<DirectCircularDependencyB>()
                .BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetRequiredService<DirectCircularDependencyA>());

            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void IndirectCircularDependency()
        {
            var expectedMessage = "A circular dependency was detected for the service of type " +
                                  $"'{GetTypeDisplayName(typeof(IndirectCircularDependencyA))}'." +
                                  Environment.NewLine +
                                  $"{GetTypeDisplayName(typeof(IndirectCircularDependencyA))} -> " +
                                  $"{GetTypeDisplayName(typeof(IndirectCircularDependencyB))} -> " +
                                  $"{GetTypeDisplayName(typeof(IndirectCircularDependencyC))} -> " +
                                  $"{GetTypeDisplayName(typeof(IndirectCircularDependencyA))}";

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IndirectCircularDependencyA>()
                .AddTransient<IndirectCircularDependencyB>()
                .AddTransient<IndirectCircularDependencyC>()
                .BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetRequiredService<IndirectCircularDependencyA>());

            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void NoCircularDependencySameTypeMultipleTimes()
        {
            var serviceProvider = new ServiceCollection()
                .AddTransient<NoCircularDependencySameTypeMultipleTimesA>()
                .AddTransient<NoCircularDependencySameTypeMultipleTimesB>()
                .AddTransient<NoCircularDependencySameTypeMultipleTimesC>()
                .BuildServiceProvider();

            var resolvedService = serviceProvider.GetRequiredService<NoCircularDependencySameTypeMultipleTimesA>();
            Assert.NotNull(resolvedService);
        }

        [Fact]
        public void DependencyOnCircularDependency()
        {
            var expectedMessage = "A circular dependency was detected for the service of type " +
                                  $"'{typeof(DirectCircularDependencyA)}'." +
                                  Environment.NewLine +
                                  $"{GetTypeDisplayName(typeof(DependencyOnCircularDependency))} -> " +
                                  $"{GetTypeDisplayName(typeof(DirectCircularDependencyA))} -> " +
                                  $"{GetTypeDisplayName(typeof(DirectCircularDependencyB))} -> " +
                                  $"{GetTypeDisplayName(typeof(DirectCircularDependencyA))}";

            var serviceProvider = new ServiceCollection()
                .AddTransient<DependencyOnCircularDependency>()
                .AddTransient<DirectCircularDependencyA>()
                .AddTransient<DirectCircularDependencyB>()
                .BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetRequiredService<DependencyOnCircularDependency>());

            Assert.Equal(expectedMessage, exception.Message);
        }
    }
}