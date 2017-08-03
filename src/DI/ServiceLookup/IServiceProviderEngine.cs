// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal interface IServiceProviderEngine : IDisposable
    {
        object GetService(Type serviceType);

        IServiceScope RootScope { get; }
        event Action<Type, IServiceCallSite> OnCreate;
        event Action<Type, IServiceScope> OnResolve;
    }
}