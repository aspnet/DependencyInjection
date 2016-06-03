// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection.Tests.Fakes;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CircularDependencyTests
    {
        [Fact]
        public void SelfCircularDependency_CannotResolve()
        {
            var serviceProvider = new ServiceCollection()
                .AddTransient<SelfCircularDependency>()
                .BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetRequiredService<SelfCircularDependency>());

            Assert.Equal(
                Resources.FormatCannotResolveService(typeof(SelfCircularDependency), typeof(SelfCircularDependency)),
                exception.Message);
        }

        [Fact]
        public void SelfCircularDependencyGenericDirect()
        {
            var serviceProvider = new ServiceCollection()
                .AddTransient<SelfCircularDependencyGeneric<string>>()
                .BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                serviceProvider.GetRequiredService<SelfCircularDependencyGeneric<string>>());

            Assert.Equal(
                Resources.FormatCannotResolveService(
                    typeof(SelfCircularDependencyGeneric<string>),
                    typeof(SelfCircularDependencyGeneric<string>)),
                exception.Message);
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

            Assert.Equal(
                Resources.FormatCannotResolveService(
                    typeof(SelfCircularDependencyGeneric<string>),
                    typeof(SelfCircularDependencyGeneric<string>)),
                exception.Message);
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

            Assert.Equal(
                Resources.FormatCannotResolveService(
                    typeof(ISelfCircularDependencyWithInterface), 
                    typeof(SelfCircularDependencyWithInterface)),
                exception.Message);
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

            Assert.Equal(
                Resources.FormatCannotResolveService(
                    typeof(DirectCircularDependencyA), 
                    typeof(DirectCircularDependencyB)),
                exception.Message);
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

            Assert.Equal(
                Resources.FormatCannotResolveService(
                    typeof(IndirectCircularDependencyA),
                    typeof(IndirectCircularDependencyC)),
                exception.Message);
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