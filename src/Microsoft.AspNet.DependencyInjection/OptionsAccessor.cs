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

namespace Microsoft.AspNet.DependencyInjection
{
    public class OptionsAccessor<TOptions> : IOptionsAccessor<TOptions> where TOptions : new()
    {
        private object _lock = new object();
        private TOptions _options;
        private IEnumerable<IOptionsSetup<TOptions>> _setups;

        public OptionsAccessor(IEnumerable<IOptionsSetup<TOptions>> setups)
        {
            _setups = setups;
        }

        public virtual TOptions SetupOptions(TOptions options)
        {
            if (_setups != null)
            {
                return _setups
                    .OrderBy(setup => setup.Order)
                    .Aggregate(
                        options,
                        (op, setup) =>
                        {
                            setup.Setup(op);
                            return op;
                        });
            }
            return options;
        }

        public virtual TOptions BuildOptions()
        {
            return SetupOptions(new TOptions());
        }

        public TOptions Options
        {
            get
            {
                lock (_lock)
                {
                    if (_options == null)
                    {
                        _options = BuildOptions();
                    }
                }
                return _options;
            }
        }
    }
}