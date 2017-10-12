// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public static class TypeNameHelper
    {
        public static string GetTypeName<T>(string genericPart = null) => GetTypeName(typeof(T), genericPart);

        public static string GetTypeName(Type type, string genericPart = null)
        {
            var name = type.FullName;

            if (type.IsGenericType)
            {
                name = name.Substring(0, name.IndexOf('`'));
            }

            if (genericPart != null)
            {
                name = $"{name}<{genericPart}>";
            }

            return name;
        }
    }
}
