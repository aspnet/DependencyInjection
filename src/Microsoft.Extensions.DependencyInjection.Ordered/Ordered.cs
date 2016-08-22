// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection.Ordered
{
    internal class Ordered<TService> : IOrdered<TService>
    {
        private readonly object _valuesLock = new object();
        private List<TService> _values;

        private readonly OrderedScopeProvider<TService>.TransientOrderedScopeProvider _transientOrderedScopeProvider;
        private readonly OrderedScopeProvider<TService>.ScopedOrderedScopeProvider _scopedOrderedScopeProvider;
        private readonly OrderedScopeProvider<TService>.SingletonOrderedScopeProvider _singletonOrderedScopeProvider;

        public Ordered(
            OrderedScopeProvider<TService>.TransientOrderedScopeProvider transientOrderedScopeProvider,
            OrderedScopeProvider<TService>.ScopedOrderedScopeProvider scopedOrderedScopeProvider,
            OrderedScopeProvider<TService>.SingletonOrderedScopeProvider singletonOrderedScopeProvider)
        {
            _transientOrderedScopeProvider = transientOrderedScopeProvider;
            _scopedOrderedScopeProvider = scopedOrderedScopeProvider;
            _singletonOrderedScopeProvider = singletonOrderedScopeProvider;
        }

        private void EnsureValues()
        {
            lock (_valuesLock)
            {
                if (_values != null)
                {
                    return;
                }

                var values = new SortedDictionary<int, TService>();
                foreach (var keyValuePair in _transientOrderedScopeProvider.GetValues())
                {
                    values.Add(keyValuePair.Key, keyValuePair.Value);
                }
                foreach (var keyValuePair in _scopedOrderedScopeProvider.GetValues())
                {
                    values.Add(keyValuePair.Key, keyValuePair.Value);
                }
                foreach (var keyValuePair in _singletonOrderedScopeProvider.GetValues())
                {
                    values.Add(keyValuePair.Key, keyValuePair.Value);
                }

                _values = values.Values.ToList();
            }
        }

        public IEnumerator<TService> GetEnumerator()
        {
            EnsureValues();
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}