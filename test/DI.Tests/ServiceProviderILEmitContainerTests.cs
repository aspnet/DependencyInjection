// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection.Specification;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class ServiceProviderILEmitContainerTests: DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection collection) =>
            collection.BuildServiceProvider(new ServiceProviderOptions() { Mode = ServiceProviderMode.ILEmit});
    }
}