// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection.ServiceLookup;
using Microsoft.Extensions.DependencyInjection.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection.Utils;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CircularDependencyTests
    {
        [Fact]
        public void SelfCircularDependency()
        {
            var serviceProvider = new ServiceCollection()
                .AddTransient<SelfCircularDependency>()
                .BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetRequiredService<SelfCircularDependency>());

            var expectedErrorMessage = new CallSiteChain()
                .AddAndSetConstructorCallImplementationType<SelfCircularDependency>()
                .CreateCircularDependencyExceptionMessage(typeof(SelfCircularDependency));

            Assert.Equal(expectedErrorMessage, exception.Message);
        }

        [Fact]
        public void SelfCircularDependencyInEnumerable()
        {
            var serviceProvider = new ServiceCollection()
                .AddTransient<SelfCircularDependency>()
                .BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetRequiredService<IEnumerable<SelfCircularDependency>>());

            var expectedErrorMessage = new CallSiteChain()
                .AddAndSetEnumerableImplementationType<SelfCircularDependency>()
                .AddAndSetConstructorCallImplementationType<SelfCircularDependency>()
                .CreateCircularDependencyExceptionMessage(typeof(SelfCircularDependency));

            Assert.Equal(expectedErrorMessage, exception.Message);
        }

        [Fact]
        public void SelfCircularDependencyGenericDirect()
        {
            var serviceProvider = new ServiceCollection()
                .AddTransient<SelfCircularDependencyGeneric<string>>()
                .BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetRequiredService<SelfCircularDependencyGeneric<string>>());

            var expectedErrorMessage = new CallSiteChain()
                .AddAndSetConstructorCallImplementationType<SelfCircularDependencyGeneric<string>>()
                .CreateCircularDependencyExceptionMessage(typeof(SelfCircularDependencyGeneric<string>));

            Assert.Equal(expectedErrorMessage, exception.Message);
        }

        [Fact]
        public void SelfCircularDependencyGenericIndirect()
        {
            var serviceProvider = new ServiceCollection()
                .AddTransient<SelfCircularDependencyGeneric<int>>()
                .AddTransient<SelfCircularDependencyGeneric<string>>()
                .BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetRequiredService<SelfCircularDependencyGeneric<int>>());

            var expectedErrorMessage = new CallSiteChain()
                .AddAndSetConstructorCallImplementationType<SelfCircularDependencyGeneric<int>>()
                .AddAndSetConstructorCallImplementationType<SelfCircularDependencyGeneric<string>>()
                .CreateCircularDependencyExceptionMessage(typeof(SelfCircularDependencyGeneric<string>));

            Assert.Equal(expectedErrorMessage, exception.Message);
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
            var serviceProvider = new ServiceCollection()
                .AddTransient<ISelfCircularDependencyWithInterface, SelfCircularDependencyWithInterface>()
                .AddTransient<SelfCircularDependencyWithInterface>()
                .BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetRequiredService<SelfCircularDependencyWithInterface>());

            var expectedErrorMessage = new CallSiteChain()
                .AddAndSetConstructorCallImplementationType<SelfCircularDependencyWithInterface>()
                .AddAndSetConstructorCallImplementationType<ISelfCircularDependencyWithInterface, SelfCircularDependencyWithInterface >()
                .CreateCircularDependencyExceptionMessage(typeof(ISelfCircularDependencyWithInterface));

            Assert.Equal(expectedErrorMessage, exception.Message);
        }

        [Fact]
        public void DirectCircularDependency()
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<DirectCircularDependencyA>()
                .AddSingleton<DirectCircularDependencyB>()
                .BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetRequiredService<DirectCircularDependencyA>());

            var expectedErrorMessage = new CallSiteChain()
                .AddAndSetConstructorCallImplementationType<DirectCircularDependencyA>()
                .AddAndSetConstructorCallImplementationType<DirectCircularDependencyB>()
                .CreateCircularDependencyExceptionMessage(typeof(DirectCircularDependencyA));

            Assert.Equal(expectedErrorMessage, exception.Message);
        }

        [Fact]
        public void IndirectCircularDependency()
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IndirectCircularDependencyA>()
                .AddTransient<IndirectCircularDependencyB>()
                .AddTransient<IndirectCircularDependencyC>()
                .BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetRequiredService<IndirectCircularDependencyA>());

            var expectedErrorMessage = new CallSiteChain()
                .AddAndSetConstructorCallImplementationType<IndirectCircularDependencyA>()
                .AddAndSetConstructorCallImplementationType<IndirectCircularDependencyB>()
                .AddAndSetConstructorCallImplementationType<IndirectCircularDependencyC>()
                .CreateCircularDependencyExceptionMessage(typeof(IndirectCircularDependencyA));

            Assert.Equal(expectedErrorMessage, exception.Message);
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