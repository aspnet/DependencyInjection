﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.DependencyInjection.Tests.Fakes;

namespace Microsoft.Framework.DependencyInjection.Tests
{
    public class ServiceAcceptingFactoryService
    {
        public ServiceAcceptingFactoryService(ScopedFactoryService scopedService,
                                              IFactoryService transientService)
        {
            ScopedService = scopedService;
            TransientService = transientService;
        }

        public ScopedFactoryService ScopedService { get; private set; }

        public IFactoryService TransientService { get; private set; }
    }
}