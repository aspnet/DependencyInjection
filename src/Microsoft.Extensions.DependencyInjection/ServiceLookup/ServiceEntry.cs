// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    public class ServiceEntry
    {
        private readonly object _sync = new object();

        public ServiceEntry(IService service)
        {
            First = service;
            Last = service;
        }

        public IService First { get; }
        public IService Last { get; private set; }

        public void Add(IService service)
        {
            lock (_sync)
            {
                Last.Next = service;
                Last = service;
            }
        }
    }
}
