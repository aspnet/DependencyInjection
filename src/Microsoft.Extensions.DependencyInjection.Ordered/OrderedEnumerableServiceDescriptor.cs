// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.DependencyInjection.Ordered
{
    public class OrderedEnumerableServiceDescriptor: EnumerableServiceDescriptor
    {
        public OrderedEnumerableServiceDescriptor(Type serviceType) : base(serviceType)
        {
        }
    }
}