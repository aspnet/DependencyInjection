// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection.Tests.Fakes;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CircularDependencyTests
    {
        [Fact]
        public void SelfCircularDependency()
        {
            var type = typeof(SelfCircularDependency);
            var expectedMessage = string.Join(Environment.NewLine,
                $"A circular dependency was detected for the service of type '{type}'.",
                "Resolution path:",
                $"Resolving '{type}' by activating '{type}'.",
                $"Resolving '{type}'.");

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
            var type = typeof(SelfCircularDependency);
            var enumerableType = typeof(IEnumerable<SelfCircularDependency>);
            var expectedMessage = string.Join(Environment.NewLine,
                $"A circular dependency was detected for the service of type '{type}'.",
                "Resolution path:",
                $"Resolving '{enumerableType}' by creating collection.",
                $"Resolving '{type}' by activating '{type}'.",
                $"Resolving '{type}'.");

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
            var type = typeof(SelfCircularDependencyGeneric<string>);
            var expectedMessage = string.Join(Environment.NewLine,
                $"A circular dependency was detected for the service of type '{type}'.",
                "Resolution path:",
                $"Resolving '{type}' by activating '{type}'.",
                $"Resolving '{type}'.");

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
            var typeString = typeof(SelfCircularDependencyGeneric<string>);
            var typeInt = typeof(SelfCircularDependencyGeneric<int>);
            var expectedMessage = string.Join(Environment.NewLine,
                $"A circular dependency was detected for the service of type '{typeString}'.",
                "Resolution path:",
                $"Resolving '{typeInt}' by activating '{typeInt}'.",
                $"Resolving '{typeString}' by activating '{typeString}'.",
                $"Resolving '{typeString}'.");

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
            var typeInterface = typeof(ISelfCircularDependencyWithInterface);
            var type = typeof(SelfCircularDependencyWithInterface);
            var expectedMessage = string.Join(Environment.NewLine,
                $"A circular dependency was detected for the service of type '{typeInterface}'.",
                "Resolution path:",
                $"Resolving '{type}' by activating '{type}'.",
                $"Resolving '{typeInterface}' by activating '{type}'.",
                $"Resolving '{typeInterface}'.");

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
            var typeA = typeof(DirectCircularDependencyA);
            var typeB = typeof(DirectCircularDependencyB);
            var expectedMessage = string.Join(Environment.NewLine,
                $"A circular dependency was detected for the service of type '{typeA}'.",
                "Resolution path:",
                $"Resolving '{typeA}' by activating '{typeA}'.",
                $"Resolving '{typeB}' by activating '{typeB}'.",
                $"Resolving '{typeA}'.");

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
            var typeA = typeof(IndirectCircularDependencyA);
            var typeB = typeof(IndirectCircularDependencyB);
            var typeC = typeof(IndirectCircularDependencyC);
            var expectedMessage = string.Join(Environment.NewLine,
                $"A circular dependency was detected for the service of type '{typeA}'.",
                "Resolution path:",
                $"Resolving '{typeA}' by activating '{typeA}'.",
                $"Resolving '{typeB}' by activating '{typeB}'.",
                $"Resolving '{typeC}' by activating '{typeC}'.",
                $"Resolving '{typeA}'.");

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
            var type = typeof(DependencyOnCircularDependency);
            var typeA = typeof(DirectCircularDependencyA);
            var typeB = typeof(DirectCircularDependencyB);
            var expectedMessage = string.Join(Environment.NewLine,
                $"A circular dependency was detected for the service of type '{typeA}'.",
                "Resolution path:",
                $"Resolving '{type}' by activating '{type}'.",
                $"Resolving '{typeA}' by activating '{typeA}'.",
                $"Resolving '{typeB}' by activating '{typeB}'.",
                $"Resolving '{typeA}'.");

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