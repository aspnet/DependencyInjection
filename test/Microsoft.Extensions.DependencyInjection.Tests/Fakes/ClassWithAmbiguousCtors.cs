// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection.Specification.Fakes;

namespace Microsoft.Extensions.DependencyInjection.Tests.Fakes
{
    public class ClassWithAmbiguousCtors
    {
        public ClassWithAmbiguousCtors(string data)
        {
        }

        public ClassWithAmbiguousCtors(IFakeService service, string data)
        {
        }

        public ClassWithAmbiguousCtors(IFakeService service, int data)
        {
        }

        public ClassWithAmbiguousCtors(IFakeService service, string data1, int data2)
        {
        }
    }
}