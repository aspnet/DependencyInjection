// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace System.Collections.Concurrent
{
    internal static class ConcurrentDictionaryExtensions
    {
        // From https://github.com/dotnet/corefx/issues/394#issuecomment-69494764
        // This lets us pass a state parameter allocation free GetOrAdd
        internal static TValue GetOrAdd<TKey, TValue, TArg>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TArg, TValue> valueFactory, TArg arg)
        {
            Debug.Assert(dictionary != null);
            Debug.Assert(key != null);
            Debug.Assert(valueFactory != null);

            while (true)
            {
                TValue value;
                if (dictionary.TryGetValue(key, out value))
                {
                    return value;
                }

                value = valueFactory(key, arg);
                if (dictionary.TryAdd(key, value))
                {
                    return value;
                }
            }
        }

    }
}
