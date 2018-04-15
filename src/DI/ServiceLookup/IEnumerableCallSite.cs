// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class IEnumerableCallSite : IServiceCallSite
    {
        internal Type ItemType { get; }
        internal IServiceCallSite[] ServiceCallSites { get; }

        public IEnumerableCallSite(Type itemType, IServiceCallSite[] serviceCallSites) : base(new ResultCache(ServiceLifetime.Transient, null))
        {
            ItemType = itemType;
            ServiceCallSites = serviceCallSites;
        }

        public override Type ServiceType => typeof(IEnumerable<>).MakeGenericType(ItemType);
        public override Type ImplementationType  => ItemType.MakeArrayType();
        public override CallSiteKind Kind { get; } = CallSiteKind.IEnumerable;
    }
}