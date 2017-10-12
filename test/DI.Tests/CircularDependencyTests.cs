// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Tests.Fakes;
using Xunit;
using static Microsoft.Extensions.DependencyInjection.Tests.TypeNameHelper;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CircularDependencyTests
    {
        private static string GetCircularDependencyErrorMessage(params string[] types)
        {
            return string.Concat($"A circular dependency was detected for the service of type '{types.Last()}'.",
                Environment.NewLine,
                string.Join(" -> ", types));
        }

        [Fact]
        public void SelfCircularDependency()
        {
            var type = GetTypeName<SelfCircularDependency>();
            var expectedMessage = GetCircularDependencyErrorMessage(type, type);

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
            var type = GetTypeName<SelfCircularDependency>();
            var enumerableType = GetTypeName<IEnumerable<SelfCircularDependency>>();
            var expectedMessage = GetCircularDependencyErrorMessage(enumerableType, type, type);

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
            var type = GetTypeName<SelfCircularDependencyGeneric<string>>();
            var expectedMessage = GetCircularDependencyErrorMessage(type, type);

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
            var typeString = GetTypeName<SelfCircularDependencyGeneric<string>>();
            var typeInt = GetTypeName<SelfCircularDependencyGeneric<int>>();
            var expectedMessage = GetCircularDependencyErrorMessage(typeInt, typeString, typeString);

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
            var typeInterface = GetTypeName<ISelfCircularDependencyWithInterface>();
            var type = GetTypeName<SelfCircularDependencyWithInterface>();
            var expectedMessage = GetCircularDependencyErrorMessage(type, $"{typeInterface}({type})", typeInterface);

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
            var typeA = GetTypeName<DirectCircularDependencyA>();
            var typeB = GetTypeName<DirectCircularDependencyB>();
            var expectedMessage = GetCircularDependencyErrorMessage(typeA, typeB, typeA);

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
            var typeA = GetTypeName<IndirectCircularDependencyA>();
            var typeB = GetTypeName<IndirectCircularDependencyB>();
            var typeC = GetTypeName<IndirectCircularDependencyC>();
            var expectedMessage = GetCircularDependencyErrorMessage(typeA, typeB, typeC, typeA);

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
            var type = GetTypeName<DependencyOnCircularDependency>();
            var typeA = GetTypeName<DirectCircularDependencyA>();
            var typeB = GetTypeName<DirectCircularDependencyB>();
            var expectedMessage = GetCircularDependencyErrorMessage(type, typeA, typeB, typeA);

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