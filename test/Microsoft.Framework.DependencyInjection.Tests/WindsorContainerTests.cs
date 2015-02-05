// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Castle.Windsor;
using Castle.MicroKernel.Lifestyle;
using Microsoft.Framework.DependencyInjection.Windsor;
using Microsoft.Framework.DependencyInjection.Tests.Fakes;

namespace Microsoft.Framework.DependencyInjection.Tests
{
    public class WindsorContainerTests : AllContainerTestsBase
    {
        protected override IServiceProvider CreateContainer()
        {
            var container = new WindsorContainer();

            container.Populate(TestServices.DefaultServices());

            container.BeginScope();

            return container.Resolve<IServiceProvider>();
        }
    }
}