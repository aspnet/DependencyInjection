// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Framework.DependencyInjection
{
    public interface IAccessor<T>
    {
        T Value { get; }
        T SetValue(T value);
        IDisposable SetSource(Func<T> access, Func<T, T> exchange);
    }
}