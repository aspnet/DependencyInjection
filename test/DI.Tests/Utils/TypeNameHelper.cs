// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public static class TypeNameHelper
    {
        private static Dictionary<Type, string> _shortNames = new Dictionary<Type, string>
        {
            { typeof(int), "int" },
            { typeof(string), "string" }
        };

        public static string GetTypeName<T>() => GetTypeName(typeof(T));

        public static string GetTypeName(Type type)
        {
            if (_shortNames.TryGetValue(type, out var shortName))
            {
                return shortName;
            }

            var name = type.FullName;

            if (type.IsGenericType)
            {
                name = name.Substring(0, name.IndexOf('`'));

                if (type.GenericTypeArguments.Any())
                {
                    var genericArgs = string.Join(",", type.GenericTypeArguments.Select(GetTypeName));
                    name = $"{name}<{genericArgs}>";
                }
            }

            return name;
        }
    }
}
