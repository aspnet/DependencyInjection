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
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.AspNet.DependencyInjection.Tests.Fakes;
using Xunit;
using System.Collections.Generic;
using Microsoft.AspNet.ConfigurationModel.Sources;

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

        //[Theory]
        //[InlineData("string", "100", "true", 100, true)]
        //[InlineData("string", "-1", "false", -1, false)]
        //[InlineData("string", "bogus", "blah", 0, false)]
        //public void CanSetupConfigOptionsWithConfigAccessor(string str, string intConfig, string boolConfig, int i, bool b)
        //{
        //    var services = new ServiceCollection();
        //    services.AddSingleton<IOptionsAccessor<FakeConfigOptions>, ConfigOptionsAccessor<FakeConfigOptions>>();
        //    var dic = new Dictionary<string, string>
        //    { 
        //        {"int", intConfig},
        //        {"string", str},
        //        {"bool", boolConfig }
        //    };
        //    var config = new Configuration { new MemoryConfigurationSource(dic) };
        //    services.AddInstance<IConfiguration>(config);

        //    var provider = services.BuildServiceProvider();

        //    var accessor = provider.GetService<IOptionsAccessor<FakeConfigOptions>>();
        //    Assert.NotNull(accessor);

        //    var options = accessor.Options;
        //    Assert.NotNull(options);
        //    Assert.Equal(str, options.String);
        //    Assert.Equal(b, options.Bool);
        //    Assert.Equal(i, options.Int);
        //}

        [Theory]
        [InlineData("string", "100", "true", 100, true)]
        [InlineData("string", "-1", "false", -1, false)]
        [InlineData("string", "bogus", "blah", 0, false)]
        public void CanSetupConfigOptionsWithSetup(string str, string intConfig, string boolConfig, int i, bool b)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IOptionsAccessor<FakeConfigOptions>, OptionsAccessor<FakeConfigOptions>>();
            services.AddSetup<ConfigOptionsSetup<FakeConfigOptions>>();
            var dic = new Dictionary<string, string>
            { 
                {"int", intConfig},
                {"string", str},
                {"bool", boolConfig }
            };
            var config = new Configuration { new MemoryConfigurationSource(dic) };
            services.AddInstance<IConfiguration>(config);

            var provider = services.BuildServiceProvider();

            var accessor = provider.GetService<IOptionsAccessor<FakeConfigOptions>>();
            Assert.NotNull(accessor);

            var options = accessor.Options;
            Assert.NotNull(options);
            Assert.Equal(str, options.String);
            Assert.Equal(b, options.Bool);
            Assert.Equal(i, options.Int);
        }
    }
}
