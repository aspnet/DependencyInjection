// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.ExceptionServices;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class CreateInstanceCallSite : IServiceCallSite
    {
        internal TypeServiceDescriptor Descriptor { get; }

        public CreateInstanceCallSite(TypeServiceDescriptor descriptor)
        {
            Descriptor = descriptor;
        }
    }
}
