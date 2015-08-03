// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !DNXCORE50
using System;
using Microsoft.Framework.DependencyInjection.Structuremap;
using Microsoft.Framework.DependencyInjection.Tests.Fakes;
using StructureMap;

namespace Microsoft.Framework.DependencyInjection.Tests
{
    public class StructureMapContainerTests : ScopingContainerTestBase
    {
        protected override IServiceProvider CreateContainer()
        {
            var container = StructuremapRegistration.Populate(TestServices.DefaultServices());
            return container.GetInstance<IServiceProvider>();
        }
    }
}

#endif
