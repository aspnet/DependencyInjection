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

        [Fact]
        public void FallbackServiceNotUsedIfFallbackProviderHasService()
        {
            var services = new ServiceCollection();
            var root = new FakeService();
            services.AddInstance(root);
            var inner = services.BuildServiceProvider();

            services = new ServiceCollection();
            var outerFallback = new FakeService();
            services.AddInstance(outerFallback, isFallback:  true);

            var outer = services.BuildServiceProvider(inner);
            Assert.Equal(root, outer.GetService<FakeService>());
        }
    }
}
