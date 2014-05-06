// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using Microsoft.AspNet.ConfigurationModel;
using Microsoft.AspNet.ConfigurationModel.Sources;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.AspNet.DependencyInjection.Tests.Fakes;
using Xunit;
using System.Collections.Generic;

namespace Microsoft.AspNet.DependencyInjection.Tests
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

        // TODO: figure out how to move this test into all container test base (config causes issues)
        [Theory]
        [InlineData("#", "100", "true", 100, true)]
        [InlineData("#", "-1", "false", -1, false)]
        [InlineData("#", "bogus", "blah", 0, false)]
        public void CanSetupConfigOptionsWithSetup(string str, string intConfig, string boolConfig, int i, bool b)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IOptionsAccessor<FakeOptions>, OptionsAccessor<FakeOptions>>();
            services.SetupOptions<FakeOptions>(o => o.Message += "a", (int)DefaultOptionSetupOrder.Framework);
            services.AddSetup<ConfigOptionsSetup<FakeOptions>>();
            services.AddSetup<FakeOptionsSetupC>();
            services.AddSetup(new FakeOptionsSetupB());
            services.AddSetup(typeof(FakeOptionsSetupA));
            services.SetupOptions<FakeOptions>(o => o.Message += "z", 10000);

            var dic = new Dictionary<string, string>
            { 
                {"INT", intConfig},
                {"message", str},
                {"bOOl", boolConfig }
            };
            var config = new Configuration { new MemoryConfigurationSource(dic) };
            services.AddInstance<IConfiguration>(config);

            var provider = services.BuildServiceProvider();

            var accessor = provider.GetService<IOptionsAccessor<FakeOptions>>();
            Assert.NotNull(accessor);

            var options = accessor.Options;
            Assert.NotNull(options);
            Assert.Equal(str+"BCz", options.Message); // This verifies that the setup changes made before config are blown away
            Assert.Equal(b, options.Bool);
            Assert.Equal(i, options.Int);
        }
    }
}
