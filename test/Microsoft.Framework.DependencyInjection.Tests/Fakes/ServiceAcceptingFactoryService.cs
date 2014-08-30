// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.DependencyInjection.Tests.Fakes;

namespace Microsoft.Framework.DependencyInjection.Tests
{
    public class ServiceAcceptingFactoryService
    {
        public ServiceAcceptingFactoryService(IFactoryService service)
        {
            FactoryService = service;
        }

        public IFactoryService FactoryService { get; private set; }
    }
}