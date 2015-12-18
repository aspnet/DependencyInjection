// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class ServiceEntry
    {
        private object _sync = new object();

        public ServiceEntry(Type serviceType)
        {
            ServiceType = serviceType;
        }

        public Type ServiceType { get; private set; }

        public IService First { get; private set; }

        public IService Last { get; private set; }

        public void Link(IService service)
        {
            if (service == null)
            {
                return;
            }

            lock (_sync)
            {
                if (service.Next != null)
                {
                    service.Next.Previous = service;
                }
                else if (Last == null)
                {
                    Last = service;
                }
                else
                {
                    service.Previous = Last;
                    Last.Next = service;
                    Last = service;
                }

                if (service.Previous != null)
                {
                    service.Previous.Next = service;
                }
                else if (First == null)
                {
                    First = service;
                }
                else
                {
                    service.Next = First;
                    First.Previous = service;
                    First = service;
                }
            }
        }

        public void Unlink(IService service)
        {
            if (service == null)
            {
                return;
            }

            lock (_sync)
            {
                if (service == First)
                {
                    First = service.Next;
                }

                if (service == Last)
                {
                    Last = service.Previous;
                }

                if (service.Next != null)
                {
                    service.Next.Previous = service.Previous;
                }

                if (service.Previous != null)
                {
                    service.Previous.Next = service.Next;
                }
            }
        }
    }
}
