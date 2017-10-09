// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.ServiceLookup;

namespace Microsoft.Extensions.DependencyInjection.Utils
{
    internal static class CallSiteChainExtensions
    {
        public static CallSiteChain AddAndSetConstructorCallImplementationType<TService, TImplementation>(this CallSiteChain callSiteChain)
        {
            return callSiteChain.AddAndSetConstructorCallImplementationType(typeof(TService), typeof(TImplementation));
        }

        public static CallSiteChain AddAndSetConstructorCallImplementationType<T>(this CallSiteChain callSiteChain)
        {
            var type = typeof(T);
            return callSiteChain.AddAndSetConstructorCallImplementationType(type, type);
        }

        public static CallSiteChain AddAndSetConstructorCallImplementationType(this CallSiteChain callSiteChain, Type serviceType, Type implementationType)
        {
            callSiteChain.SetConstructorCallImplementationType(serviceType, implementationType);
            return callSiteChain;
        }

        public static CallSiteChain AddAndSetEnumerableImplementationType<T>(this CallSiteChain callSiteChain)
        {
            return callSiteChain.AddAndSetEnumerableImplementationType(typeof(T));
        }

        public static CallSiteChain AddAndSetEnumerableImplementationType(this CallSiteChain callSiteChain, Type type)
        {
            type = typeof(IEnumerable<>).MakeGenericType(type);
            callSiteChain.SetEnumerableImplementationType(type);
            return callSiteChain;
        }
    }
}
