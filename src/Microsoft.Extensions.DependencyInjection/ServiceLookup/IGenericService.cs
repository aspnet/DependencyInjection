// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal interface IGenericService
    {
        ServiceLifetime Lifetime { get; }

        IService GetService(Type closedServiceType);
    }
}
