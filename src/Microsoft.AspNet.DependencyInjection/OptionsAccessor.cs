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
    public class OptionsAccessor<TOptions> : IOptionsAccessor<TOptions> where TOptions : new()
    {
        private object _lock = new object();
        private TOptions _options;
        private IEnumerable<IOptionsSetup<TOptions>> _setups;
        private IConfiguration _config;

        public OptionsAccessor(IEnumerable<IOptionsSetup<TOptions>> setups, IConfiguration config)
        {
            _setups = setups;
            _config = config;
        }

        public TOptions Options
        {
            get
            {
                lock (_lock)
                {
                    if (_options == null)
                    {
                        _options = new TOptions();
                        Read(_options, _config);
                        if (_setups != null)
                        {
                            _options = _setups
                                .OrderBy(setup => setup.Order)
                                .Aggregate(
                                    _options,
                                    (options, setup) =>
                                    {
                                        setup.Setup(options);
                                        return options;
                                    });
                            // Consider: null out setups without creating race condition?
                        }
                    }
                }
                return _options;
            }
        }

        public static void Read(object obj, IConfiguration config)
        {
            if (config == null || obj == null)
            {
                return;
            }
            var type = obj.GetType();
            var props = type.GetTypeInfo().DeclaredProperties;
            foreach (var prop in props)
            {
                if (!prop.CanWrite)
                {
                    continue;
                }
                var configValue = config.Get(prop.Name);
                if (configValue == null)
                {
                    continue;
                }

                try
                {
                    prop.SetValue(obj, Convert.ChangeType(configValue, prop.PropertyType));
                }
                catch
                {
                    // TODO: what do we do about errors?
                }
            }
        }
    }
}