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
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNet.ConfigurationModel;

namespace Microsoft.AspNet.DependencyInjection
{
    public class ConfigOptionsAccessor<TOptions> : OptionsAccessor<TOptions> where TOptions : IConfigOptions,new()
    {
        private object _lock = new object();
        private TOptions _options;
        private IEnumerable<IOptionsSetup<TOptions>> _setups;
        private IConfiguration _config;

        public ConfigOptionsAccessor(IEnumerable<IOptionsSetup<TOptions>> setups, IEnumerable<IConfiguration> config) : base(setups)
        {
            _setups = setups;
            _config = config.FirstOrDefault();
        }

        public override  TOptions BuildOptions()
        {
            var options = new TOptions();
            options.ReadProperties(_config);
            return SetupOptions(options);
        }
    }
}