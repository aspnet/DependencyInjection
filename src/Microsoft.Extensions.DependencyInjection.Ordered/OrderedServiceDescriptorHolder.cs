// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Extensions.DependencyInjection.Ordered
{
    internal class OrderedServiceDescriptorHolder<T>
    {
        public OrderedServiceDescriptorHolder(OrderedServiceDescriptor serviceDescriptor)
        {
            ServiceDescriptor = serviceDescriptor;
        }

        public OrderedServiceDescriptor ServiceDescriptor { get; }
    }
}