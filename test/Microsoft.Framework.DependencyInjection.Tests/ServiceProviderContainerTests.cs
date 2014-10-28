// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.DependencyInjection.Tests.Fakes;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Framework.DependencyInjection.Tests
{
    public class ServiceProviderContainerTests : ScopingContainerTestBase
    {
        protected override IServiceProvider CreateContainer()
        {
            return CreateContainer(new FakeFallbackServiceProvider());
        }

        protected override IServiceProvider CreateContainer(IServiceProvider fallbackProvider)
        {
            return TestServices.DefaultServices().BuildServiceProvider(fallbackProvider);
        }

        [Theory]
        [InlineData(OverrideMode.DefaultMany)]
        [InlineData(OverrideMode.DefaultSingle)]
        [InlineData(OverrideMode.OverrideMany)]
        [InlineData(OverrideMode.OverrideSingle)]
        public void CanSingleServiceAnymode(OverrideMode mode)
        {
            var services = new ServiceCollection();
            services.AddTransient<FakeService>(mode);

            var provider = services.BuildServiceProvider();
            Assert.NotNull(provider.GetService<FakeService>());
        }

        [Fact]
        public void OverrideSingleWins()
        {
            var services = new ServiceCollection();
            var defaultSingle = new FakeService();
            var defaultMany = new FakeService();
            var overrideSingle = new FakeService();
            var overrideMany = new FakeService();
            services.AddInstance<FakeService>(defaultSingle, OverrideMode.DefaultSingle);
            services.AddInstance<FakeService>(defaultMany, OverrideMode.DefaultMany);
            services.AddInstance<FakeService>(overrideSingle, OverrideMode.OverrideSingle);
            services.AddInstance<FakeService>(overrideMany, OverrideMode.OverrideMany);

            var provider = services.BuildServiceProvider();
            Assert.Equal(overrideSingle, provider.GetService<FakeService>());
        }

        [Fact]
        public void OverrideManyWinsOverDefaults()
        {
            var services = new ServiceCollection();
            var defaultSingle = new FakeService();
            var defaultMany = new FakeService();
            var overrideMany = new FakeService();
            services.AddInstance<FakeService>(defaultSingle, OverrideMode.DefaultSingle);
            services.AddInstance<FakeService>(defaultMany, OverrideMode.DefaultMany);
            services.AddInstance<FakeService>(overrideMany, OverrideMode.OverrideMany);
            services.AddInstance<FakeService>(overrideMany, OverrideMode.OverrideMany);

            var provider = services.BuildServiceProvider();
            Assert.Equal(overrideMany, provider.GetService<FakeService>());
            var many = provider.GetService<IEnumerable<FakeService>>();
            Assert.Equal(2, many.Count());
        }

        [Fact]
        public void OverrideSingleBlocksIEnumerable()
        {
            var services = new ServiceCollection();
            var overrideSingle = new FakeService();
            var overrideMany = new FakeService();
            services.AddInstance<FakeService>(overrideSingle, OverrideMode.OverrideSingle);
            services.AddInstance<FakeService>(overrideMany, OverrideMode.OverrideMany);

            var provider = services.BuildServiceProvider();
            Assert.Equal(1, provider.GetService<IEnumerable<FakeService>>().Count());
        }

        [Fact]
        public void DefaultSingleWinsOverDefaultMany()
        {
            var services = new ServiceCollection();
            var defaultSingle = new FakeService();
            var defaultMany = new FakeService();
            services.AddInstance<FakeService>(defaultSingle, OverrideMode.DefaultSingle);
            services.AddInstance<FakeService>(defaultMany, OverrideMode.DefaultMany);

            var provider = services.BuildServiceProvider();
            Assert.Equal(defaultSingle, provider.GetService<FakeService>());
        }

        [Fact]
        public void DefaultManyAllowsIEnumerable()
        {
            var services = new ServiceCollection();
            var defaultMany = new FakeService();
            services.AddInstance<FakeService>(defaultMany, OverrideMode.DefaultMany);
            services.AddInstance<FakeService>(defaultMany, OverrideMode.DefaultMany);

            var provider = services.BuildServiceProvider();
            Assert.Equal(defaultMany, provider.GetService<FakeService>());
            var many = provider.GetService<IEnumerable<FakeService>>();
            Assert.Equal(2, many.Count());
        }


    }
}
