// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class OpenIEnumerableService : IGenericService
    {
        private readonly ServiceTable _table;

        public OpenIEnumerableService(ServiceTable table)
        {
            _table = table;
        }

        public ServiceLifetime Lifetime => ServiceLifetime.Transient;

        public IService GetService(Type closedServiceType)
        {
            var itemType = closedServiceType.GetTypeInfo().GenericTypeArguments[0];

            ServiceEntry entry;

            if (_table.TryGetEntry(itemType, out entry))
            {
                return new ClosedIEnumerableService(entry);
            }

            return null;
        }
    }
}
