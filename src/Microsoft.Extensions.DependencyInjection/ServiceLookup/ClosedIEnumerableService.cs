// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class ClosedIEnumerableService : IService
    {
        private readonly ServiceEntry _serviceEntry;

        public ClosedIEnumerableService(ServiceEntry entry)
        {
            _serviceEntry = entry;
        }

        public IService Previous { get; set; }

        public IService Next { get; set; }

        public ServiceLifetime Lifetime => ServiceLifetime.Transient;

        public IServiceCallSite CreateCallSite(ServiceProvider provider)
        {
            var list = new List<Tuple<IService, IServiceCallSite>>();
            var service = _serviceEntry.First;
            while (service != null)
            {
                try
                {
                    _serviceEntry.Unlink(service);
                    var callSite = provider.GetResolveCallSite(service);
                    list.Add(new Tuple<IService, IServiceCallSite>(service, callSite));
                }
                finally
                {
                    _serviceEntry.Link(service);
                }

                service = service.Next;
            }
            return new CallSite(_serviceEntry, list.ToArray());
        }

        private class CallSite : IServiceCallSite
        {
            private readonly ServiceEntry _entry;
            private readonly Tuple<IService, IServiceCallSite>[] _serviceCallSites;

            public CallSite(ServiceEntry entry, Tuple<IService, IServiceCallSite>[] serviceCallSites)
            {
                _entry = entry;
                _serviceCallSites = serviceCallSites;
            }

            public object Invoke(ServiceProvider provider)
            {
                var array = Array.CreateInstance(_entry.ServiceType, _serviceCallSites.Length);
                for (var index = 0 ; index < _serviceCallSites.Length; index++)
                {
                    var item = _serviceCallSites[index];
                    try
                    {
                        _entry.Unlink(item.Item1);
                        array.SetValue(item.Item2.Invoke(provider), index);
                    }
                    finally
                    {
                        _entry.Link(item.Item1);
                    }
                }
                return array;
            }

            public Expression Build(Expression provider)
            {
                return Expression.NewArrayInit(
                    _entry.ServiceType,
                    _serviceCallSites.Select(callSite =>
                        Expression.Convert(
                            callSite.Item2.Build(provider),
                            _entry.ServiceType)));
            }
        }
    }
}
