// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public class EnumerableServiceDescriptor : ServiceDescriptor
    {
        public EnumerableServiceDescriptor(Type serviceType) : base(serviceType, ServiceLifetime.Transient)
        {
        }

        public List<ServiceDescriptor> Descriptors { get; } = new List<ServiceDescriptor>();

        internal override Type GetImplementationType()
        {
            return null;
        }
    }
}