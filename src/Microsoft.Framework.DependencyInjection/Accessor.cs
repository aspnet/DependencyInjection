// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Framework.DependencyInjection
{
    public class Accessor<T> : IAccessor<T>
    {
        private ContextSource _source;
        private T _value;

        public Accessor()
        {
            _source = new ContextSource();
        }

        public T Value
        {
            get
            {
                return _source.Access != null ? _source.Access() : _value;
            }
        }

        public T SetValue(T value)
        {
            if (_source.Exchange != null)
            {
                return _source.Exchange(value);
            }
            var prior = _value;
            _value = value;
            return prior;
        }

        public IDisposable SetSource(Func<T> access, Func<T, T> exchange)
        {
            var prior = _source;
            _source = new ContextSource(access, exchange);
            return new Disposable(this, prior);
        }

        struct ContextSource
        {
            public ContextSource(Func<T> access, Func<T, T> exchange)
            {
                Access = access;
                Exchange = exchange;
            }

            public readonly Func<T> Access;
            public readonly Func<T, T> Exchange;
        }

        class Disposable : IDisposable
        {
            private readonly Accessor<T> _contextAccessor;
            private readonly ContextSource _source;

            public Disposable(Accessor<T> contextAccessor, ContextSource source)
            {
                _contextAccessor = contextAccessor;
                _source = source;
            }

            public void Dispose()
            {
                _contextAccessor._source = _source;
            }
        }
    }
}