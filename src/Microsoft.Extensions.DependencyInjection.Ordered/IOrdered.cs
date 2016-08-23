// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection.Ordered
{
    public interface IOrdered<out T> : IEnumerable<T>
    {
    }
}