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
                Resources.FormatCircularDependencyException(type),
                Resources.ResolutionPathHeader,
                Resources.FormatResolutionPathItemConstructorCall(type, type),
                Resources.FormatResolutionPathItemCurrent(type));
            
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
                Resources.FormatCircularDependencyException(type),
                Resources.ResolutionPathHeader,
                Resources.FormatResolutionPathItemEnumerableCreate(enumerableType),
                Resources.FormatResolutionPathItemConstructorCall(type, type),
                Resources.FormatResolutionPathItemCurrent(type));

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
                Resources.FormatCircularDependencyException(type),
                Resources.ResolutionPathHeader,
                Resources.FormatResolutionPathItemConstructorCall(type, type),
                Resources.FormatResolutionPathItemCurrent(type));

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
                Resources.FormatCircularDependencyException(typeString),
                Resources.ResolutionPathHeader,
                Resources.FormatResolutionPathItemConstructorCall(typeInt, typeInt),
                Resources.FormatResolutionPathItemConstructorCall(typeString, typeString),
                Resources.FormatResolutionPathItemCurrent(typeString));

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

            // This will not throw because we are creating an instace of the first time
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
                Resources.FormatCircularDependencyException(typeInterface),
                Resources.ResolutionPathHeader,
                Resources.FormatResolutionPathItemConstructorCall(type, type),
                Resources.FormatResolutionPathItemConstructorCall(typeInterface, type),
                Resources.FormatResolutionPathItemCurrent(typeInterface));

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
                Resources.FormatCircularDependencyException(typeA),
                Resources.ResolutionPathHeader,
                Resources.FormatResolutionPathItemConstructorCall(typeA, typeA),
                Resources.FormatResolutionPathItemConstructorCall(typeB, typeB),
                Resources.FormatResolutionPathItemCurrent(typeA));

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
                Resources.FormatCircularDependencyException(typeA),
                Resources.ResolutionPathHeader,
                Resources.FormatResolutionPathItemConstructorCall(typeA, typeA),
                Resources.FormatResolutionPathItemConstructorCall(typeB, typeB),
                Resources.FormatResolutionPathItemConstructorCall(typeC, typeC),
                Resources.FormatResolutionPathItemCurrent(typeA));

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
    }
}