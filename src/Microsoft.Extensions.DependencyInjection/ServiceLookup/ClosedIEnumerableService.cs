// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class ClosedIEnumerableService : IService
    {
        private readonly Type _itemType;
        private readonly IList<IService> _services;

        public ClosedIEnumerableService(Type itemType, IEnumerable<IService> services)
        {
            _itemType = itemType;
            _services = services.ToList();
        }

        public IService Next { get; set; }

        public ServiceLifetime Lifetime
        {
            get { return ServiceLifetime.Transient; }
        }

        public Type ServiceType => _itemType;

        public IServiceCallSite CreateCallSite(ServiceProvider provider, ISet<Type> callSiteChain)
        {
            if (_services.Count == 0)
            {
                var item = Array.CreateInstance(_itemType, 0);
                return new EmptyIEnumerableCallSite(_itemType, item);
            }
            var list = new List<IServiceCallSite>();
            foreach (var service in _services)
            {
                list.Add(provider.GetResolveCallSite(service, callSiteChain));
            }
            return new ClosedIEnumerableCallSite(_itemType, list.ToArray());
        }
    }
}
