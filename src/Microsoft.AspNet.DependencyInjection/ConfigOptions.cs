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

using Microsoft.AspNet.ConfigurationModel;
using System;
using System.Reflection;

namespace Microsoft.AspNet.DependencyInjection
{
    public class ConfigOptions : IConfigOptions
    {
        public virtual void ReadProperties(IConfiguration config)
        {
            if (config == null)
            {
                return;
            }
            var props = GetType().GetTypeInfo().DeclaredProperties;
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
// No convert on portable
#if NET45 || K10
                    prop.SetValue(this, Convert.ChangeType(configValue, prop.PropertyType));
#endif
                }
                catch
                {
                    // TODO: what do we do about errors?
                }
            }
        }
    }
}